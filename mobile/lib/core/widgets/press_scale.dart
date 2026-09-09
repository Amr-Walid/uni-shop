import 'package:flutter/material.dart';

/// Scales its child down slightly while pressed, then releases with the
/// website's easing curve.
///
/// The storefront communicates interactivity with `transform: translateY(-6px)`
/// on `:hover`. Touch has no hover state, so the equivalent affordance is a
/// press response — without one, tiles feel inert next to the site.
///
/// Deliberately cheap:
///   * animates only `scale`, which is a transform on the existing layer — no
///     relayout, no repaint of the subtree, no shadow or blur work;
///   * drives one `AnimationController` per card, disposed with the widget;
///   * wraps the child in a `RepaintBoundary` so the scaling tile does not
///     invalidate the rest of the scrolling list.
///
/// A `translateY` lift was considered to mirror the site exactly, but moving a
/// tile upward inside a scroll view fights the scroll gesture and can clip
/// against the neighbouring row. Scale reads the same and stays inside bounds.
class PressScale extends StatefulWidget {
  const PressScale({
    super.key,
    required this.child,
    this.onTap,
    this.pressedScale = 0.97,
  });

  final Widget child;
  final VoidCallback? onTap;

  /// How far to shrink while held. 0.97 is the smallest value that still reads
  /// as a response on a phone; below ~0.94 it looks like the tile is collapsing.
  final double pressedScale;

  @override
  State<PressScale> createState() => _PressScaleState();
}

class _PressScaleState extends State<PressScale>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    // Fast in, so the press feels immediate.
    duration: const Duration(milliseconds: 110),
    // Slower out, matching the site's 0.35s settle.
    reverseDuration: const Duration(milliseconds: 220),
    vsync: this,
  );

  late final Animation<double> _scale = Tween<double>(
    begin: 1,
    end: widget.pressedScale,
  ).animate(CurvedAnimation(
    parent: _controller,
    // The site's `cubic-bezier(0.4, 0, 0.2, 1)` — Material's standard easing.
    curve: Curves.easeInOutCubic,
    reverseCurve: Curves.easeOutCubic,
  ));

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _setPressed(bool pressed) {
    if (!mounted) return;
    if (pressed) {
      _controller.forward();
    } else {
      _controller.reverse();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Listener(
      // Listener rather than GestureDetector for the press states: the child
      // subtree contains its own tappable buttons (add-to-cart, wishlist), and
      // a competing GestureDetector here would win the arena and swallow them.
      // Listener observes pointers without claiming the gesture.
      onPointerDown: (_) => _setPressed(true),
      onPointerUp: (_) => _setPressed(false),
      onPointerCancel: (_) => _setPressed(false),
      child: AnimatedBuilder(
        animation: _scale,
        // Built once and reused: the child does not depend on the animation,
        // so rebuilding it every frame would be wasted work.
        child: RepaintBoundary(child: widget.child),
        builder: (context, child) => Transform.scale(
          scale: _scale.value,
          filterQuality: FilterQuality.low,
          child: child,
        ),
      ),
    );
  }
}
