"""Describe intact generated frames without modifying PNGs or Unity import metadata."""
import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "ArtSources/EssentialStates20261112"
SHEETS = {
    "FabiusSerafim_Walk_Defense_12f": ("Fabius", "Serafim Carmesim", "Walk", "Guard"),
    "FabiusSerafim_Hurt_Death_12f": ("Fabius", "Serafim Carmesim", "Hurt", "Dead"),
    "LuciusArconte_Walk_Defense_12f": ("Lucius", "Arconte", "Walk", "Guard"),
    "LuciusArconte_Hurt_Death_12f": ("Lucius", "Arconte", "Hurt", "Dead"),
}


def main():
    entries = []
    for name, (character, form, first, second) in SHEETS.items():
        path = DEST / (name + ".png")
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        with Image.open(path) as image:
            if image.mode != "RGBA" or image.width % 3 or image.height % 4:
                raise ValueError(f"Expected native RGBA 3-column/4-row sheet: {path}")
            alpha = image.getchannel("A")
            if alpha.getextrema()[0] != 0:
                raise ValueError(f"No transparent background: {path}")
            width, height = image.width // 3, image.height // 4
            frames = []
            for index in range(12):
                row, col = divmod(index, 3)
                left, top = col * width, row * height
                # Read the alpha of the entire cell for diagnostics only.
                cell = alpha.crop((left, top, left + width, top + height))
                bounds = cell.getbbox()
                if bounds is None:
                    raise ValueError(f"Empty frame {index}: {path}")
                frames.append({
                    "id": f"{name}_{index:02d}",
                    "action": first if index < 6 else second,
                    "frameIndex": index % 6,
                    "sourceRectTopLeft": [left, top, width, height],
                    "unityRect": [left, image.height - top - height, width, height],
                    "durationMs": 120 if index >= 6 and second == "Dead" else 100,
                    "loop": index < 6 and first == "Walk",
                    "alphaBoundsInCell": list(bounds),
                    "alphaTouchesCellEdge": (bounds[0] == 0 or bounds[1] == 0
                                             or bounds[2] == width or bounds[3] == height),
                })
            entry = {
                "image": path.name, "imageSha256": digest,
                "character": character, "form": form,
                "nativeSize": list(image.size), "gridColumnsRows": [3, 4],
                "status": "visual-reviewed", "imported": False, "runtimeVerified": False,
                "userApproval": name == "FabiusSerafim_Hurt_Death_12f",
                "reference": "User character attachments, 2026-11-12",
                "direction": "unassigned-single-view", "timings": "provisional",
                "pending": ["direction and foot-pivot calibration", "cell-edge VFX review",
                            "Unity import preserving existing IDs", "render and state-transition tests"],
                "frames": frames,
            }
        (DEST / (name + ".json")).write_text(
            json.dumps(entry, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        entries.append(entry)
    (DEST / "manifest.json").write_text(json.dumps({
        "schemaVersion": 1, "pixelsModified": False,
        "notes": "48 whole frames; one view per character, not eight-direction coverage."
                 " Guard is a visual clip, not a new combat/parry mechanic.",
        "sheets": [{key: value for key, value in entry.items() if key != "frames"}
                   for entry in entries],
    }, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({"sheets": len(entries), "wholeFrames": sum(len(e["frames"]) for e in entries),
                      "imported": False, "pixelsModified": False}))


if __name__ == "__main__":
    main()
