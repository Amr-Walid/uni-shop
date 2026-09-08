#!/usr/bin/env python3
"""Generates the Android and iOS launcher icons for Uni-Shop.

The icon is the cart mark from `FTD.Web/wwwroot/images/logo-unishop.svg`
(viewBox 0 0 190 56) on the brand purple, drawn with the same geometry so the
launcher icon, the in-app `AppLogo` widget and the website logo are the same
artwork.

Why generate rather than commit a design export: the Flutter template ships
its own placeholder `ic_launcher.png` in five densities plus fifteen iOS sizes.
Hand-editing twenty files is where drift and half-updated sets come from, and
the previous state (a stock Flutter icon on the home screen, which is what the
device screenshot showed) is exactly that failure. Re-running this script
regenerates every size from one definition.

Usage:  python3 mobile/tool/generate_launcher_icons.py
Requires: Pillow
"""

from __future__ import annotations

import os
from functools import lru_cache
from pathlib import Path

from PIL import Image, ImageDraw

# ── Brand ────────────────────────────────────────────────────────────────────
PURPLE = (51, 0, 119, 255)      # --primary  #330077
WHITE = (255, 255, 255, 255)


REPO = Path(__file__).resolve().parents[2]
ANDROID_RES = REPO / "mobile/android/app/src/main/res"
IOS_ASSETS = REPO / "mobile/ios/Runner/Assets.xcassets/AppIcon.appiconset"

ANDROID_DENSITIES = {
    "mipmap-mdpi": 48,
    "mipmap-hdpi": 72,
    "mipmap-xhdpi": 96,
    "mipmap-xxhdpi": 144,
    "mipmap-xxxhdpi": 192,
}

# The full set Xcode expects for a modern iOS app icon.
IOS_SIZES = [
    ("Icon-App-20x20@1x.png", 20), ("Icon-App-20x20@2x.png", 40), ("Icon-App-20x20@3x.png", 60),
    ("Icon-App-29x29@1x.png", 29), ("Icon-App-29x29@2x.png", 58), ("Icon-App-29x29@3x.png", 87),
    ("Icon-App-40x40@1x.png", 40), ("Icon-App-40x40@2x.png", 80), ("Icon-App-40x40@3x.png", 120),
    ("Icon-App-60x60@2x.png", 120), ("Icon-App-60x60@3x.png", 180),
    ("Icon-App-76x76@1x.png", 76), ("Icon-App-76x76@2x.png", 152),
    ("Icon-App-83.5x83.5@2x.png", 167),
    ("Icon-App-1024x1024@1x.png", 1024),
]


SOURCE_LOGO = REPO / "FTD.Web/wwwroot/images/logo-unishop-icon.png"

# The purple ink in the source artwork (#452F91). Sampled from the file rather
# than assumed: that PNG predates the current CSS tokens, so it is NOT
# --primary #330077.
LOGO_INK = (69, 47, 145)


@lru_cache(maxsize=2)
def _load_mark(light: bool) -> Image.Image:
    """Loads the website's designed logo, cropped to its ink.

    Cached: this is called once per icon size (27 of them) and the recolour
    pass below walks every pixel of the source.

    An earlier version of this script re-drew the mark from the SVG's path
    data. That was a mistake: the SVG in the repo is a rough approximation
    (a thin open stroke), whereas the real brand mark shipped as PNG is a
    solid "US" cart monogram — the artwork actually visible on the site.
    Compositing the genuine asset makes the launcher icon match the site
    rather than merely resemble it.

    When `light` is set the purple ink is recoloured to white in place,
    instead of loading the committed `logo-unishop-icon-light.png`. That
    variant was produced by hard-keying the purple, so its edges are aliased
    and its antialiased boundary pixels became visible white fringing when
    scaled down. Rewriting only the RGB channels and leaving alpha untouched
    preserves the original's smooth edges. The orange cart furniture is
    deliberately left alone, exactly as the site keeps it.

    Cropping to the alpha bounding box means the padding maths applies to the
    ink, not to whatever transparent margin the export happened to include.
    """
    img = Image.open(SOURCE_LOGO).convert("RGBA")
    px = img.load()
    w, h = img.size

    # The source was exported over a white page, which left a hard, FULLY
    # OPAQUE white halo two-to-three pixels wide between the ink and the
    # transparent margin (verified by sampling a scanline: the ink ends at a
    # single blended pixel, then three pixels of pure white at alpha 255, then
    # alpha 0). Invisible against the site's white background, that halo reads
    # as a bright fringe the moment the mark is composited onto the purple
    # launcher-icon background — which is exactly the artefact that appeared.
    #
    # Turning those pixels transparent is what actually fixes it; recolouring
    # the ink alone does not, because the halo is not ink.
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            if r > 235 and g > 235 and b > 235:
                px[x, y] = (r, g, b, 0)

    if light:
        for y in range(h):
            for x in range(w):
                r, g, b, a = px[x, y]
                if a == 0:
                    continue
                # Tolerant enough to catch the antialiased ramp toward the
                # purple, tight enough to spare the orange furniture.
                if (abs(r - LOGO_INK[0]) < 70
                        and abs(g - LOGO_INK[1]) < 70
                        and abs(b - LOGO_INK[2]) < 80):
                    px[x, y] = (255, 255, 255, a)

    bbox = img.split()[-1].getbbox()
    return img.crop(bbox) if bbox else img


def draw_mark(size: int, *, padding_ratio: float, bg, light_mark: bool, rounded: bool) -> Image.Image:
    """Composites the brand mark, centred, onto a `size`x`size` icon."""
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))

    if bg is not None:
        if rounded:
            # iOS masks the icon itself, but a rounded source stays correct if
            # it is ever displayed unmasked.
            layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
            ImageDraw.Draw(layer).rounded_rectangle(
                [0, 0, size - 1, size - 1], radius=int(size * 0.22), fill=bg)
            canvas = Image.alpha_composite(canvas, layer)
        else:
            canvas = Image.alpha_composite(
                canvas, Image.new("RGBA", (size, size), bg))

    mark = _load_mark(light=light_mark)
    avail = size * (1 - 2 * padding_ratio)
    scale = min(avail / mark.width, avail / mark.height)
    target = (max(1, round(mark.width * scale)), max(1, round(mark.height * scale)))

    # LANCZOS because every launcher size is a downscale of the 384px source;
    # NEAREST/BILINEAR leave the diagonal cart edges visibly stepped.
    mark = mark.resize(target, Image.LANCZOS)

    canvas.alpha_composite(
        mark, ((size - target[0]) // 2, (size - target[1]) // 2))
    return canvas


def main() -> None:
    written = 0

    # ── Android legacy icon (square, purple background) ──────────────────────
    for folder, size in ANDROID_DENSITIES.items():
        out = ANDROID_RES / folder
        out.mkdir(parents=True, exist_ok=True)
        img = draw_mark(size, padding_ratio=0.18, bg=PURPLE, light_mark=True, rounded=False)
        img.save(out / "ic_launcher.png")
        written += 1

    # ── Android round icon ───────────────────────────────────────────────────
    # Referenced by AndroidManifest's android:roundIcon and used by launchers
    # on circular-icon devices. It must exist in every density the legacy icon
    # does, or the manifest reference fails to resolve at build time.
    for folder, size in ANDROID_DENSITIES.items():
        out = ANDROID_RES / folder
        img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        # Circular purple plate, then the mark inset far enough to stay clear
        # of the curve.
        ImageDraw.Draw(img).ellipse([0, 0, size - 1, size - 1], fill=PURPLE)
        mark = draw_mark(size, padding_ratio=0.26, bg=None, light_mark=True, rounded=False)
        img.alpha_composite(mark)
        img.save(out / "ic_launcher_round.png")
        written += 1

    # ── Android adaptive foreground ──────────────────────────────────────────
    # Adaptive icons are cropped to a safe zone of the inner 66%, so the
    # foreground needs far more padding than the legacy icon or the launcher
    # will clip the cart. Background is a flat colour layer (see values/).
    for folder, size in ANDROID_DENSITIES.items():
        out = ANDROID_RES / folder
        img = draw_mark(int(size * 1.5), padding_ratio=0.29, bg=None, light_mark=True, rounded=False)
        img.save(out / "ic_launcher_foreground.png")
        written += 1

    # ── iOS ──────────────────────────────────────────────────────────────────
    if IOS_ASSETS.exists():
        for name, size in IOS_SIZES:
            # iOS icons must be fully opaque — a transparent icon is rejected
            # at submission, so the purple background is always drawn.
            img = draw_mark(size, padding_ratio=0.18, bg=PURPLE, light_mark=True, rounded=False)
            img.convert("RGB").save(IOS_ASSETS / name)
            written += 1
    else:
        print(f"! iOS asset catalogue not found at {IOS_ASSETS}, skipped")

    # ── Play Store / marketing ───────────────────────────────────────────────
    store = REPO / "mobile/assets/branding"
    store.mkdir(parents=True, exist_ok=True)
    draw_mark(512, padding_ratio=0.18, bg=PURPLE, light_mark=True, rounded=False).save(
        store / "icon-512.png")
    draw_mark(1024, padding_ratio=0.18, bg=None, light_mark=False, rounded=False).save(
        store / "mark-1024-transparent.png")
    written += 2

    print(f"Wrote {written} icon files.")


if __name__ == "__main__":
    main()
