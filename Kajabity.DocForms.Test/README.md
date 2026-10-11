Kajabity.DocForms.Test
======================

A suite of NUnit 4 tests for Kajabity.DocForms, targeting .NET Framework 4.8
and .NET 10 for Windows.

## Running the tests

From PowerShell at the repository root, with a .NET SDK and the .NET Framework 4.8
targeting pack installed:

```powershell
dotnet test Kajabity.DocForms.Test\Kajabity.DocForms.Test.csproj -c Release --settings Kajabity.DocForms.Test\DocForms.runsettings
```

For the save/close fixture, append `--filter TestCategory=Issue3` (64 cases).
For window-close cancellation and successful closure, use
`--filter TestCategory=Issue4` (13 cases).
For just the disk safety cases, append `--filter TestCategory=SaveSafety`
(12 cases). The full suite currently has 111 cases. `DocForms.runsettings` makes
zero-test runs an error, including filtered runs.

All projects use SDK-style project files. The library and tests target both
.NET Framework 4.8 and .NET 10 for Windows. PlainTextEditor targets .NET Framework
4.8; the other samples target .NET 10 for Windows.
The .NET 10 test target compiles the PlainTextEditor form sources and resource
into the test assembly so the existing form tests run on both frameworks.
To restore and build the full solution:

```powershell
./.github/scripts/restore-packages.ps1
dotnet build Kajabity.DocForms.sln -c Release --no-restore
```

## Save and close coverage

`Forms/SaveBeforeCloseTest.cs` exercises the real form commands with stubbed prompts
and selectors. Its manager writes real files in a unique temporary directory for
each test, including a partial write before simulated failures. Fixtures run on an
STA thread, dispose forms directly, and clean up their files even after assertions
fail. The six sample-form tests also dispose their forms and isolate save output.

Coverage includes:

- Cancelled Save As, failed saves, cancelled confirmation, successful saves,
  discarding edits and unmodified documents before Close, New, Open and Recent Files.
- Preserving document identity, contents, filename, name, new-file flag and modified
  state when closing cannot proceed; checking the `AttemptCloseDocument` result.
- Cancelling window closure after a cancelled confirmation, cancelled Save As or
  write failure.
- Application `FormClosing` handlers vetoing closure while the original document
  is still available, preserving modified and unmodified documents without prompting.
- Already-cancelled closing events preserving the document while still running
  application handlers, even if a handler clears the incoming cancellation.
- Successful window closure for unmodified documents, discarded edits and saved
  documents, with one close and no duplicate prompts or saves.
- Repeated saves with backups enabled and disabled, replacing an existing backup,
  and leaving unrelated `.tmp` files untouched.
- Save As to new and existing destinations, committing the final name only after
  replacement, and checking actual saved bytes and temporary-file cleanup.
- Partial write failures and locked destinations, preserving original and backup
  contents, retaining unsaved state, and successfully retrying after unlocking.

The issue #4 regression checks failed all 13 cases before the lifecycle fix;
the updated full suite passes all 111 cases.

## Follow-up coverage

Still worth testing: read-only destinations/backups, missing directories, disk-full
and network failures, and application-wide Exit with multiple windows. The fixture
invokes `OnFormClosing` directly and does not call process-wide `Application.Exit()`.

Save overrides should write the supplied filename and then call `base.Save(filename)`.
The form's safe-save path defers that base status update until replacement succeeds;
direct calls to `Manager.Save` retain their existing behaviour. Overrides that change
document state themselves or write multiple files need separate review.

Full documentation is available at [Kajabity Tools](http://www.kajabity.com/kajabity-tools/).
