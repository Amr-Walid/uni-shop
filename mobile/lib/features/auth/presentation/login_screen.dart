import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/error/app_exception.dart';
import '../../../core/l10n/app_localizations.dart';
import '../../../core/providers/core_providers.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/validators.dart';
import '../../../routing/routes.dart';
import 'auth_controller.dart';

/// Combined sign-in / register screen.
///
/// One screen with a toggle rather than two routes: the fields overlap almost
/// entirely, and a user who mistyped their email on registration should be
/// able to switch to sign-in without losing what they typed.
class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key, this.redirectTo});

  /// Where to go after a successful sign-in — the route that triggered the
  /// redirect, so the user lands where they intended.
  final String? redirectTo;

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmController = TextEditingController();
  final _nameController = TextEditingController();
  final _phoneController = TextEditingController();

  bool _isRegisterMode = false;
  bool _obscurePassword = true;
  bool _isSubmitting = false;
  AppException? _error;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _confirmController.dispose();
    _nameController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) return;

    FocusScope.of(context).unfocus();
    setState(() {
      _isSubmitting = true;
      _error = null;
    });

    try {
      final controller = ref.read(authControllerProvider.notifier);

      if (_isRegisterMode) {
        await controller.register(
          email: _emailController.text,
          password: _passwordController.text,
          fullName: _nameController.text,
          phone: _phoneController.text.trim().isEmpty
              ? null
              : Validators.normalisePhone(_phoneController.text),
        );
      } else {
        await controller.login(
          email: _emailController.text,
          password: _passwordController.text,
        );
      }

      if (!mounted) return;
      // The router's redirect also handles this, but navigating explicitly
      // means the user is not left on the form for a frame.
      context.go(widget.redirectTo ?? Routes.home);
    } on AppException catch (error) {
      if (!mounted) return;
      setState(() {
        _error = error;
        _isSubmitting = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          _isRegisterMode ? l10n.t('authRegister') : l10n.t('authLogin'),
        ),
      ),
      body: AbsorbPointer(
        absorbing: _isSubmitting,
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.all(AppSpacing.lg),
            children: [
              Text(
                l10n.t('authGuestPrompt'),
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.textTheme.bodySmall?.color,
                ),
              ),

              const SizedBox(height: AppSpacing.xl),

              if (_error != null) ...[
                Container(
                  padding: const EdgeInsets.all(AppSpacing.md),
                  decoration: BoxDecoration(
                    color: AppColors.danger.withOpacity(0.1),
                    borderRadius: AppRadius.cardRadius,
                  ),
                  child: Row(
                    children: [
                      const Icon(
                        Icons.error_outline,
                        color: AppColors.danger,
                        size: 20,
                      ),
                      const SizedBox(width: AppSpacing.sm),
                      Expanded(
                        child: Text(
                          // The server's message is already localized and
                          // specific ("wrong credentials" vs "account
                          // locked"), so it beats any generic copy here.
                          _error!.message,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: AppColors.danger,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: AppSpacing.lg),
              ],

              if (_isRegisterMode) ...[
                TextFormField(
                  controller: _nameController,
                  textInputAction: TextInputAction.next,
                  decoration: InputDecoration(
                    labelText: l10n.t('fieldFullName'),
                    prefixIcon: const Icon(Icons.person_outline),
                  ),
                  validator: (value) =>
                      Validators.minLength(value, 3, l10n),
                ),
                const SizedBox(height: AppSpacing.md),
              ],

              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                textInputAction: TextInputAction.next,
                textDirection: TextDirection.ltr,
                autofillHints: const [AutofillHints.email],
                decoration: InputDecoration(
                  labelText: l10n.t('fieldEmail'),
                  prefixIcon: const Icon(Icons.email_outlined),
                ),
                validator: (value) => Validators.email(value, l10n),
              ),

              if (_isRegisterMode) ...[
                const SizedBox(height: AppSpacing.md),
                TextFormField(
                  controller: _phoneController,
                  keyboardType: TextInputType.phone,
                  textInputAction: TextInputAction.next,
                  textDirection: TextDirection.ltr,
                  decoration: InputDecoration(
                    labelText:
                        '${l10n.t('fieldPhone')} ${l10n.t('optional')}',
                    prefixIcon: const Icon(Icons.phone_outlined),
                  ),
                  validator: (value) =>
                      Validators.phone(value, l10n, isRequired: false),
                ),
              ],

              const SizedBox(height: AppSpacing.md),

              TextFormField(
                controller: _passwordController,
                obscureText: _obscurePassword,
                textInputAction: _isRegisterMode
                    ? TextInputAction.next
                    : TextInputAction.done,
                autofillHints: [
                  _isRegisterMode
                      ? AutofillHints.newPassword
                      : AutofillHints.password,
                ],
                decoration: InputDecoration(
                  labelText: l10n.t('authPassword'),
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    onPressed: () => setState(
                      () => _obscurePassword = !_obscurePassword,
                    ),
                    icon: Icon(
                      _obscurePassword
                          ? Icons.visibility_outlined
                          : Icons.visibility_off_outlined,
                    ),
                  ),
                ),
                validator: (value) => Validators.password(value, l10n),
                onFieldSubmitted:
                    _isRegisterMode ? null : (_) => _submit(),
              ),

              if (_isRegisterMode) ...[
                const SizedBox(height: AppSpacing.md),
                TextFormField(
                  controller: _confirmController,
                  obscureText: _obscurePassword,
                  textInputAction: TextInputAction.done,
                  decoration: InputDecoration(
                    labelText: l10n.t('authConfirmPassword'),
                    prefixIcon: const Icon(Icons.lock_outline),
                  ),
                  validator: (value) => Validators.confirmPassword(
                    value,
                    _passwordController.text,
                    l10n,
                  ),
                  onFieldSubmitted: (_) => _submit(),
                ),
              ],

              if (!_isRegisterMode) ...[
                const SizedBox(height: AppSpacing.sm),
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: TextButton(
                    onPressed: _showForgotPassword,
                    child: Text(l10n.t('authForgotPassword')),
                  ),
                ),
              ],

              const SizedBox(height: AppSpacing.lg),

              FilledButton(
                onPressed: _isSubmitting ? null : _submit,
                child: _isSubmitting
                    ? const SizedBox(
                        width: 22,
                        height: 22,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : Text(
                        _isRegisterMode
                            ? l10n.t('authRegister')
                            : l10n.t('authLogin'),
                      ),
              ),

              const SizedBox(height: AppSpacing.lg),

              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    _isRegisterMode
                        ? l10n.t('authHaveAccount')
                        : l10n.t('authNoAccount'),
                    style: theme.textTheme.bodySmall,
                  ),
                  TextButton(
                    onPressed: () => setState(() {
                      _isRegisterMode = !_isRegisterMode;
                      _error = null;
                    }),
                    child: Text(
                      _isRegisterMode
                          ? l10n.t('authLogin')
                          : l10n.t('authRegister'),
                    ),
                  ),
                ],
              ),

              const Divider(height: AppSpacing.xxl),

              // Guest browsing is a first-class path, not a fallback: the
              // store is fully shoppable without an account.
              OutlinedButton(
                onPressed: () => context.go(Routes.home),
                child: Text(l10n.t('authContinueAsGuest')),
              ),
            ],
          ),
        ),
      ),
    );
  }

  /// Takes no BuildContext parameter on purpose: it uses `State.context`, so
  /// the `mounted` checks after each await actually guard the context being
  /// used (a passed-in context would not be covered by State.mounted).
  Future<void> _showForgotPassword() async {
    final l10n = context.l10n;
    final emailController =
        TextEditingController(text: _emailController.text);

    final email = await showDialog<String>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.t('authForgotPassword')),
        content: TextField(
          controller: emailController,
          keyboardType: TextInputType.emailAddress,
          textDirection: TextDirection.ltr,
          decoration: InputDecoration(labelText: l10n.t('fieldEmail')),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(l10n.t('cancel')),
          ),
          FilledButton(
            onPressed: () =>
                Navigator.of(context).pop(emailController.text.trim()),
            child: Text(l10n.t('confirm')),
          ),
        ],
      ),
    );

    emailController.dispose();

    if (email == null || email.isEmpty || !mounted) return;

    try {
      await ref.read(authRepositoryProvider).forgotPassword(email);
    } catch (_) {
      // Swallowed deliberately. The server always reports success regardless
      // of whether the address exists, and surfacing a client-side failure
      // differently would reintroduce the enumeration oracle it avoids.
    }

    if (!mounted) return;

    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(content: Text(l10n.t('authResetSent'))),
      );
  }
}
