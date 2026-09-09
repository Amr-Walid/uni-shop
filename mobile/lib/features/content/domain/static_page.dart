import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';

/// A CMS page (terms, privacy, about). Mirrors `ContentPageDto`.
///
/// Rendered from the API rather than bundled into the app so legal wording can
/// be corrected without a store release — and so the store listings can point
/// at a live privacy-policy URL.
///
/// Like `AttributeFilterGroupDto`, this DTO predates the mobile endpoints and
/// ships BOTH language columns, so the choice is made client-side here.
class StaticPage extends Equatable {
  const StaticPage({
    required this.id,
    required this.slug,
    required this.titleAr,
    required this.titleEn,
    this.bodyAr,
    this.bodyEn,
    this.isPublished = true,
    this.updatedAt,
  });

  final int id;
  final String slug;
  final String titleAr;
  final String titleEn;
  final String? bodyAr;
  final String? bodyEn;
  final bool isPublished;
  final DateTime? updatedAt;

  String title(String lang) {
    if (lang == 'en') {
      return titleEn.trim().isNotEmpty ? titleEn : titleAr;
    }
    return titleAr.trim().isNotEmpty ? titleAr : titleEn;
  }

  /// Page body, falling back across languages.
  ///
  /// Content is authored as HTML in the admin panel. It is NOT rendered as
  /// HTML here — the app strips it to text, because an admin-authored script
  /// tag rendered in a WebView would execute with the app's privileges.
  String? body(String lang) {
    final preferred = lang == 'en' ? bodyEn : bodyAr;
    final fallback = lang == 'en' ? bodyAr : bodyEn;
    final chosen = (preferred?.trim().isNotEmpty ?? false)
        ? preferred
        : fallback;
    return (chosen?.trim().isEmpty ?? true) ? null : chosen;
  }

  factory StaticPage.fromJson(Map<String, dynamic> json) => StaticPage(
        id: Json.integer(json, 'id'),
        slug: Json.str(json, 'slug'),
        titleAr: Json.str(json, 'titleAr'),
        titleEn: Json.str(json, 'titleEn'),
        bodyAr: Json.strOrNull(json, 'bodyAr'),
        bodyEn: Json.strOrNull(json, 'bodyEn'),
        isPublished: Json.boolean(json, 'isPublished', fallback: true),
        updatedAt: Json.dateTime(json, 'updatedAt'),
      );

  @override
  List<Object?> get props => [id, slug, isPublished];
}

/// Store contact details and social links.
///
/// Every field is nullable because the API honours the admin's per-channel
/// visibility flags: a channel hidden on the website is omitted here too, so
/// the app must render only what it actually receives.
class ContactInfo extends Equatable {
  const ContactInfo({
    this.phone,
    this.phone2,
    this.email,
    this.address,
    this.city,
    this.mapEmbedUrl,
    this.workingHours,
    this.facebook,
    this.instagram,
    this.whatsapp,
    this.tiktok,
  });

  final String? phone;
  final String? phone2;
  final String? email;

  /// Already localized by the server.
  final String? address;
  final String? city;
  final String? mapEmbedUrl;
  final String? workingHours;

  final String? facebook;
  final String? instagram;
  final String? whatsapp;
  final String? tiktok;

  bool get hasSocial =>
      facebook != null ||
      instagram != null ||
      whatsapp != null ||
      tiktok != null;

  bool get hasAnyContact =>
      phone != null || phone2 != null || email != null || address != null;

  /// Full address line, skipping absent parts.
  String? get formattedAddress {
    final parts = [address, city]
        .where((p) => p != null && p.trim().isNotEmpty)
        .toList();
    return parts.isEmpty ? null : parts.join('، ');
  }

  /// `wa.me` deep link for the WhatsApp button.
  ///
  /// Non-digits are stripped and a local `01…` number is promoted to the
  /// `20…` international form, because wa.me rejects anything else and the
  /// admin field accepts free text.
  String? get whatsappUrl {
    final raw = whatsapp?.trim();
    if (raw == null || raw.isEmpty) return null;

    var digits = raw.replaceAll(RegExp(r'[^0-9]'), '');
    if (digits.isEmpty) return null;
    if (digits.startsWith('0')) digits = '20${digits.substring(1)}';

    return 'https://wa.me/$digits';
  }

  factory ContactInfo.fromJson(Map<String, dynamic> json) {
    // Social links are nested one level down.
    final social = Json.object(json, 'social') ?? const <String, dynamic>{};

    return ContactInfo(
      phone: Json.strOrNull(json, 'phone'),
      phone2: Json.strOrNull(json, 'phone2'),
      email: Json.strOrNull(json, 'email'),
      address: Json.strOrNull(json, 'address'),
      city: Json.strOrNull(json, 'city'),
      mapEmbedUrl: Json.strOrNull(json, 'mapEmbedUrl'),
      workingHours: Json.strOrNull(json, 'workingHours'),
      facebook: Json.strOrNull(social, 'facebook'),
      instagram: Json.strOrNull(social, 'instagram'),
      whatsapp: Json.strOrNull(social, 'whatsapp'),
      tiktok: Json.strOrNull(social, 'tiktok'),
    );
  }

  @override
  List<Object?> get props => [phone, phone2, email, address, whatsapp];
}
