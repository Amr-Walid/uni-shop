import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_spacing.dart';

/// Every remote image in the app goes through this widget.
///
/// It exists so three things are guaranteed everywhere:
///   1. images are disk-cached (product images are re-shown constantly while
///      scrolling a grid and must not be refetched),
///   2. a null or blank URL degrades to a branded placeholder instead of
///      throwing or rendering a grey void,
///   3. memory decode size is capped — a 2000px admin upload decoded at full
///      size in a 180px card is ~16 MB of RAM per tile and will OOM a mid-range
///      Android device after a few screens of scrolling.
class AppNetworkImage extends StatelessWidget {
  const AppNetworkImage({
    super.key,
    required this.url,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
    this.borderRadius,
    this.placeholderEmoji,
    this.backgroundColor,
  });

  final String? url;
  final double? width;
  final double? height;
  final BoxFit fit;
  final BorderRadius? borderRadius;

  /// Product/category emoji from the API, used as a friendlier placeholder
  /// than a generic icon when the image is missing.
  final String? placeholderEmoji;

  final Color? backgroundColor;

  bool get _hasUrl => url != null && url!.trim().isNotEmpty;

  @override
  Widget build(BuildContext context) {
    final radius = borderRadius ?? BorderRadius.zero;

    final child = _hasUrl
        ? CachedNetworkImage(
            imageUrl: url!,
            width: width,
            height: height,
            fit: fit,
            // Decode at roughly the display size. Multiplied by the device
            // pixel ratio so the image is still crisp on a 3x screen.
            memCacheWidth: _cacheWidth(context),
            fadeInDuration: const Duration(milliseconds: 180),
            placeholder: (_, __) => _Placeholder(
              width: width,
              height: height,
              backgroundColor: backgroundColor,
            ),
            errorWidget: (_, __, ___) => _Placeholder(
              width: width,
              height: height,
              emoji: placeholderEmoji,
              backgroundColor: backgroundColor,
              // A failed load is distinct from "still loading": show the
              // fallback content rather than a shimmer that never resolves.
              showIcon: placeholderEmoji == null,
            ),
          )
        : _Placeholder(
            width: width,
            height: height,
            emoji: placeholderEmoji,
            backgroundColor: backgroundColor,
            showIcon: placeholderEmoji == null,
          );

    return radius == BorderRadius.zero
        ? child
        : ClipRRect(borderRadius: radius, child: child);
  }

  int? _cacheWidth(BuildContext context) {
    if (width == null || !width!.isFinite) return null;
    final ratio = MediaQuery.devicePixelRatioOf(context);
    return (width! * ratio).round();
  }
}

class _Placeholder extends StatelessWidget {
  const _Placeholder({
    this.width,
    this.height,
    this.emoji,
    this.backgroundColor,
    this.showIcon = false,
  });

  final double? width;
  final double? height;
  final String? emoji;
  final Color? backgroundColor;
  final bool showIcon;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final fill = backgroundColor ??
        (isDark ? AppColors.surfaceAltDark : AppColors.surfaceAlt);

    Widget? content;
    if (emoji != null && emoji!.isNotEmpty) {
      content = Text(
        emoji!,
        style: TextStyle(fontSize: _emojiSize),
      );
    } else if (showIcon) {
      content = Icon(
        Icons.image_outlined,
        size: _emojiSize,
        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
      );
    }

    return Container(
      width: width,
      height: height,
      color: fill,
      alignment: Alignment.center,
      child: content,
    );
  }

  /// Scales the glyph to the tile so a 48px avatar and a 320px hero both look
  /// deliberate rather than using one fixed size.
  double get _emojiSize {
    final basis = [width, height]
        .whereType<double>()
        .where((v) => v.isFinite)
        .fold<double?>(null, (min, v) => min == null || v < min ? v : min);
    if (basis == null) return 32;
    return (basis * 0.35).clamp(16, 64);
  }
}

/// Square product image with the app's standard corner radius.
class ProductImage extends StatelessWidget {
  const ProductImage({
    super.key,
    required this.url,
    this.emoji,
    this.size,
    this.borderRadius,
  });

  final String? url;
  final String? emoji;
  final double? size;
  final BorderRadius? borderRadius;

  @override
  Widget build(BuildContext context) {
    return AspectRatio(
      aspectRatio: AppSizes.productImageAspect,
      child: AppNetworkImage(
        url: url,
        width: size,
        height: size,
        placeholderEmoji: emoji,
        borderRadius: borderRadius ?? AppRadius.cardRadius,
      ),
    );
  }
}
