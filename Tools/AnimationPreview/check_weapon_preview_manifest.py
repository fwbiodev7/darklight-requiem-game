"""Check the complete-figure viewer's data and local references without touching Assets."""
from pathlib import Path
import argparse
import collections
import json
import math
import struct
import urllib.parse

ROOT = Path(__file__).resolve().parents[2]


def check(manifest):
    manifest = manifest.resolve()
    errors, warnings, duplicates = [], [], []

    def pairs(items):
        result = {}
        for key, value in items:
            if key in result:
                duplicates.append(key)
            result[key] = value
        return result

    data = json.loads(manifest.read_text(encoding="utf-8-sig"), object_pairs_hook=pairs)
    errors.extend("Duplicate JSON key: " + str(key) for key in duplicates)
    frames, clips = data.get("frames", {}), data.get("clips", [])
    if not isinstance(frames, dict) or not frames:
        raise ValueError("frames must be a nonempty object")
    if not isinstance(clips, list) or not clips:
        raise ValueError("clips must be a nonempty array")
    images = {}

    def image_info(value, label):
        if not isinstance(value, str) or not value:
            errors.append(label + ": missing local PNG path")
            return None
        if value in images:
            return images[value]
        parsed = urllib.parse.urlsplit(value)
        if parsed.scheme or parsed.netloc:
            errors.append(label + ": image must reference a local file: " + value)
            return None
        path = (manifest.parent / urllib.parse.unquote(parsed.path)).resolve()
        if not path.is_relative_to(ROOT):
            errors.append(label + ": image escapes the project: " + value)
            return None
        if not path.is_file():
            errors.append(label + ": missing file: " + value)
            return None
        with path.open("rb") as stream:
            header = stream.read(24)
        if len(header) != 24 or header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
            errors.append(label + ": invalid PNG header: " + value)
            return None
        images[value] = struct.unpack(">II", header[16:24])
        return images[value]

    def finite(value):
        return isinstance(value, (float, int)) and not isinstance(value, bool) and math.isfinite(value)

    for key, frame in frames.items():
        label = "Frame " + key
        if not isinstance(frame, dict):
            errors.append(label + ": frame must be an object")
            continue
        size = image_info(frame.get("image"), label)
        rect = frame.get("rect")
        if not isinstance(rect, list) or len(rect) != 4 or not all(finite(v) for v in rect):
            errors.append(label + ": invalid rect")
        elif min(rect[:2]) < 0 or min(rect[2:]) <= 0:
            errors.append(label + ": negative origin or empty rect")
        elif size and (rect[0] + rect[2] > size[0] or rect[1] + rect[3] > size[1]):
            errors.append(label + ": rect exceeds PNG bounds")
        if not finite(frame.get("ppu")) or frame["ppu"] <= 0:
            errors.append(label + ": invalid pixels per unit")
        pivot = frame.get("pivot")
        if not isinstance(pivot, list) or len(pivot) != 2 or not all(finite(v) for v in pivot):
            errors.append(label + ": invalid pivot")
        if frame.get("mask"):
            mask_size = image_info(frame["mask"], label + " mask")
            if mask_size and size and mask_size != size:
                errors.append(label + ": selector and image dimensions differ")
        if not frame.get("sourceName"):
            warnings.append(label + ": no source sprite name recorded")

    ids, referenced = set(), set()
    context_actions = collections.defaultdict(set)
    contexts, actions, weapons, presentations = collections.Counter(), collections.Counter(), collections.Counter(), collections.Counter()
    compared, changed = 0, 0
    for clip in clips:
        if not isinstance(clip, dict):
            errors.append("Clip must be an object")
            continue
        key = clip.get("id")
        if not key or key in ids:
            errors.append("Missing/duplicate clip id: " + str(key))
        ids.add(key)
        context = tuple(clip.get(field) for field in ("skin", "weapon", "form", "direction"))
        contexts[context] += 1
        context_actions[context].add(clip.get("action"))
        actions[str(clip.get("action"))] += 1
        weapons[str(clip.get("weapon"))] += 1
        streams = {}
        for version in ("current", "legacy"):
            sequence = clip.get(version)
            if not isinstance(sequence, list):
                errors.append(str(key) + ": " + version + " must be a list")
                continue
            if not sequence:
                warnings.append(str(key) + ": no " + version + " frames")
            names = []
            for index, item in enumerate(sequence):
                if not isinstance(item, dict):
                    errors.append(str(key) + ": invalid " + version + " frame " + str(index))
                    continue
                frame_id = item.get("frame")
                if frame_id not in frames:
                    errors.append(str(key) + ": unknown frame: " + str(frame_id))
                else:
                    referenced.add(frame_id)
                    names.append(frame_id)
                    presentations[str(frames[frame_id].get("presentation"))] += 1
                if not finite(item.get("durationMs")) or item["durationMs"] <= 0:
                    errors.append(str(key) + ": invalid duration at " + version + "/" + str(index))
            streams[version] = names
        if not streams.get("current") and not streams.get("legacy"):
            errors.append(str(key) + ": no playable frames in either version")
        if streams.get("current") and streams.get("legacy"):
            compared += 1
            changed += streams["current"] != streams["legacy"]
    if all(all(type(value) is int for value in context) for context in contexts):
        expected = {(skin, weapon, form, direction)
                    for skin in range(2) for weapon in range(3 + (skin == 1))
                    for form in range(4) for direction in range(8)}
        for context in sorted(expected - contexts.keys()):
            errors.append("Missing skin/weapon/form/direction context: " + str(context))
        for context in sorted(contexts.keys() - expected):
            errors.append("Unsupported skin/weapon/form/direction context: " + str(context))
        for context, present in context_actions.items():
            for required in ("Idle", "Walk"):
                if required not in present:
                    errors.append("Missing " + required + " in context " + str(context))
            if context[1] < 3:
                for required in ("Combo:combo", "Combo:pause", "Combo:heavy"):
                    if required not in present:
                        errors.append("Missing " + required + " in context " + str(context))
    return dict(
        ok=not errors, manifest=str(manifest.relative_to(ROOT)), frames=len(frames), referencedFrames=len(referenced),
        clips=len(clips), images=len(images), contexts=len(contexts), comparableClips=compared,
        clipsWithDifferentFrameSequences=changed, clipsByWeapon=dict(weapons), clipsByAction=dict(actions),
        referencedPresentations=dict(presentations), unusedFrames=len(set(frames)-referenced), errors=errors,
        warnings=warnings, evidence="Static local viewer data/reference audit. Does not certify artwork, shader equivalence, runtime selection or gameplay.",
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("manifest", nargs="?", type=Path, default=ROOT / "outputs/previa-armas-20261024/manifest.json")
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    try:
        report = check(args.manifest)
    except (OSError, ValueError, TypeError, KeyError, struct.error) as error:
        report = dict(ok=False, errors=[str(error)], warnings=[])
    report_path = args.report or args.manifest.with_name("data-qa.json")
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    compact = {k: v for k, v in report.items() if k not in ("errors", "warnings")}
    compact.update(errors=report["errors"][:8], errorCount=len(report["errors"]), warnings=report["warnings"][:5], warningCount=len(report["warnings"]))
    print(json.dumps(compact, ensure_ascii=True))
    return 0 if report["ok"] else 2


if __name__ == "__main__":
    raise SystemExit(main())
