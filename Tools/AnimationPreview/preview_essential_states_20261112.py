"""Preview intact source cells; never change source pixels or Unity metadata."""
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSources/EssentialStates20261112"
OUT = ROOT / "outputs/essential-states-20261112/previews"


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    clips = []
    for descriptor in sorted(SOURCE.glob("*_12f.json")):
        data = json.loads(descriptor.read_text(encoding="utf-8"))
        source = SOURCE / data["image"]
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        if digest != data["imageSha256"]:
            raise ValueError(f"Source hash mismatch: {source.name}")
        with Image.open(source) as image:
            if image.mode != "RGBA" or list(image.size) != data["nativeSize"]:
                raise ValueError(f"Source format mismatch: {source.name}")
            for action in dict.fromkeys(f["action"] for f in data["frames"]):
                frames = sorted((f for f in data["frames"] if f["action"] == action),
                                key=lambda f: f["frameIndex"])
                if [f["frameIndex"] for f in frames] != list(range(6)):
                    raise ValueError(f"Expected six sequential whole frames: {source.name}/{action}")
                canvases, diagnostics = [], []
                for frame in frames:
                    x, y, width, height = frame["sourceRectTopLeft"]
                    if width <= 0 or height <= 0 or x < 0 or y < 0 or x + width > image.width or y + height > image.height:
                        raise ValueError(f"Invalid whole-cell rectangle: {frame['id']}")
                    cell = image.crop((x, y, x + width, y + height))
                    alpha = cell.getchannel("A")
                    edge_pixels = alpha.crop((0, 0, width, 1)).tobytes()
                    edge_pixels += alpha.crop((0, height - 1, width, height)).tobytes()
                    edge_pixels += alpha.crop((0, 1, 1, height - 1)).tobytes()
                    edge_pixels += alpha.crop((width - 1, 1, width, height - 1)).tobytes()
                    diagnostics.append({"id": frame["id"], "alphaBounds": alpha.getbbox(),
                                        "edgePixelsAlphaOver0": sum(a > 0 for a in edge_pixels),
                                        "edgePixelsAlphaOver127": sum(a > 127 for a in edge_pixels)})
                    # Native-sized whole frames on two backgrounds. No resampling,
                    # cleanup, cropping of anatomy, or modification of the source.
                    canvas = Image.new("RGB", (width * 2, height + 32), "#181820")
                    canvas.paste("#dedede", (width, 0, width * 2, height))
                    canvas.paste(cell, (0, 0), alpha)
                    canvas.paste(cell, (width, 0), alpha)
                    ImageDraw.Draw(canvas).text((8, height + 8),
                        f"{action}  {frame['frameIndex'] + 1}/6  |  source preview, not Unity", fill="white")
                    canvases.append(canvas)
                name = source.stem.replace("_12f", "").replace("_Walk_Defense", "").replace("_Hurt_Death", "") + "_" + action
                gif = OUT / (name + ".gif")
                options = {"loop": 0} if frames[0]["loop"] else {}
                canvases[0].save(gif, save_all=True, append_images=canvases[1:],
                    duration=[f["durationMs"] for f in frames], disposal=2, optimize=False, **options)
                with Image.open(gif) as check:
                    if check.n_frames != 6:
                        raise ValueError(f"GIF lost a frame: {gif.name}")
                clips.append({"preview": gif.name, "source": source.name, "sourceSha256": digest,
                              "action": action, "frames": 6, "loop": frames[0]["loop"],
                              "durationMs": sum(f["durationMs"] for f in frames),
                              "direction": data["direction"], "diagnostics": diagnostics})
        if hashlib.sha256(source.read_bytes()).hexdigest() != digest:
            raise ValueError(f"Source changed during preview: {source.name}")
    if len(clips) != 8:
        raise ValueError(f"Expected eight clips, found {len(clips)}")
    report = {"sourcePixelsModified": False, "unityImported": False, "runtimeVerified": False,
              "notes": ["GIF previews quantize colors; PNG sources remain authoritative.",
                        "Edge alpha is a review signal, not proof of clipped anatomy.",
                        "Directions and foot pivots remain uncalibrated; no runtime catalog changed."],
              "clips": clips}
    (OUT / "preview-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"clips": len(clips), "frames": sum(c["frames"] for c in clips),
                      "sourcePixelsModified": False, "unityImported": False,
                      "output": str(OUT)}))


if __name__ == "__main__":
    main()
