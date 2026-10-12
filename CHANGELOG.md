# Changelog

All notable changes to Kajabity.DocForms are recorded here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Release dates are omitted where they have not been verified.

## [Unreleased]

## [0.4.0]

### Added

- A `net10.0-windows` build alongside the existing `net48` build in one library and NuGet package.
- Opt-in `WindowPlacementManager`, serialisable `WindowPlacement` and testable display calculations for restoring independent top-level windows.
- Regression tests for application close cancellation and window placement.
- Package icon, XML API documentation, Source Link symbols and updated public guides.

### Changed

- Converted the library, tests and retained samples to SDK-style projects.
- Form-closing subscribers now see the document before KDF closes it, and their cancellation is honoured.
- HexViewer, CsvEditor and JavaPropertiesEditor target .NET 10 for Windows; PlainTextEditor remains on .NET Framework 4.8.

### Fixed

- Application `FormClosing` cancellation is preserved even if a later handler clears the flag.

### Removed

- The obsolete `SDIForm` class and its TextEditor demonstration project. This breaks applications still deriving from `SDIForm`; use `SingleDocumentForm<TDocument>` and `SingleDocumentManager<TDocument>`.

## [0.3.1]

- Latest published .NET Framework 4.8 package and the baseline for [MIGRATION.md](MIGRATION.md).
- Cancelled Save As or failed saves retain the open modified document. Interactive saves use a sibling temporary file and preserve the destination on write failure.

## [0.3.0]

- Earlier published .NET Framework 4.8 package. See its [NuGet version history](https://www.nuget.org/packages/Kajabity.DocForms/#versions-body-tab) and repository tag for release details.

Earlier 0.1 and 0.2 release history remains in [Kajabity.com's 0.2 release post](https://www.kajabity.com/2017/08/kajabity-docforms-0-2-released/) and the repository tags. It has not been reconstructed here without a release-by-release audit.
