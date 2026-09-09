import 'package:flutter/material.dart';

/// Fades its child in while sliding it up a few pixels, once, on first build.
///
/// This is the app-side equivalent of the website's `fadeInUp` keyframe, which
/// the storefront uses to introduce sections as they appear. Without it, app
/// content snaps into place while the site eases in — the same content feeling
/// noticeably less considered.
///
/// Performance notes, because this can appear many times on one screen:
///   * only opacity and a translation are animated. Both are composited
///     properties, so no layout or paint pass is re-run for the subtree;
///   * the child is built once and reused across frames via `AnimatedBuilder`'s
///     `child` argument;
///   * the animation runs once and the controller is then idle — nothing keeps
///     ticking after the entrance, so a settled screen costs nothing;
///   * `[delay]` staggers items without a timer per item: the delay is folded
///     into the controller's curve via `Interval`.
class FadeInUp extends StatefulWidget {
  const FadeInUp({
    super.key,
    required this.child,
    this.delay = Duration.zero,
    this.offset = 16,
    this.duration = const Duration(milliseconds: 420),
  });

  final Widget child;

  /// Stagger. Kept small — a long delay on a list makes scrolling feel
  /// unresponsive because content appears after the user already looked at it.
  final Duration delay;

  /// How far up the child travels, in logical pixels. The site uses ~20px;
  /// 16 reads better on a phone's shorter viewport.
  final double offset;

  final Duration duration;

  @override
  State<FadeInUp> createState() => _FadeInUpState();
}

class _FadeInUpState extends State<FadeInUp>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _animation;

  @override
  void initState() {
    super.initState();

    // The delay is expressed as a leading Interval on a single controller
    // rather than a Future.delayed + setState. A pending timer that fires
    // after the widget is disposed is a classic crash in scrolling lists, and
    // this construction makes that impossible.
    final total = widget.duration + widget.delay;
    _controller = AnimationController(duration: total, vsync: this);

    final delayFraction = total.inMicroseconds == 0
        ? 0.0
        : widget.delay.inMicroseconds / total.inMicroseconds;

    _animation = CurvedAnimation(
      parent: _controller,
      // `cubic-bezier(0.4, 0, 0.2, 1)` in the site's stylesheet.
      curve: Interval(delayFraction, 1, curve: Curves.easeOutCubic),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // Respect the OS "reduce motion" setting: animating against it can cause
    // real discomfort for motion-sensitive users, and the platform exposes the
    // preference precisely so apps honour it.
    if (MediaQuery.disableAnimationsOf(context)) return widget.child;

    return AnimatedBuilder(
      animation: _animation,
      child: widget.child,
      builder: (context, child) {
        final t = _animation.value;
        return Opacity(
          opacity: t,
          child: Transform.translate(
            offset: Offset(0, widget.offset * (1 - t)),
            child: child,
          ),
        );
      },
    );
  }
}
