Kajabity.DocForms.Test
======================

A suite of NUnit 4 tests for Kajabity.DocForms, targeting .NET Framework 4.8 on Windows.

## Running the tests

From PowerShell at the repository root, with a .NET SDK and the .NET Framework 4.8
targeting pack installed:

```powershell
dotnet test Kajabity.DocForms.Test\Kajabity.DocForms.Test.csproj -c Release --settings Kajabity.DocForms.Test\DocForms.runsettings
```

For the save/close fixture, append `--filter TestCategory=Issue3` (54 cases).
For just the disk safety cases, append `--filter TestCategory=SaveSafety`
(12 cases). The full suite currently has 77 cases. `DocForms.runsettings` makes
zero-test runs an error, including filtered runs.

The production library and sample projects still use their existing .NET Framework
project files. To build the full solution, restore the legacy sample packages first:

```powershell
./.github/scripts/restore-packages.ps1
msbuild Kajabity.DocForms.sln /p:Configuration=Release
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
- Repeated saves with backups enabled and disabled, replacing an existing backup,
  and leaving unrelated `.tmp` files untouched.
- Save As to new and existing destinations, committing the final name only after
  replacement, and checking actual saved bytes and temporary-file cleanup.
- Partial write failures and locked destinations, preserving original and backup
  contents, retaining unsaved state, and successfully retrying after unlocking.

The review baseline was 29 failures and 33 passes; the updated suite passes all 77.

## Follow-up coverage

Still worth testing: read-only destinations/backups, missing directories, disk-full
and network failures, and application-wide Exit with multiple windows. The fixture
does not call process-wide `Application.Exit()`. Also consider a separate
`FormClosing` subscriber cancelling after this class has already closed the
document: the current event order does not preserve the document in that case.

Save overrides should write the supplied filename and then call `base.Save(filename)`.
The form's safe-save path defers that base status update until replacement succeeds;
direct calls to `Manager.Save` retain their existing behaviour. Overrides that change
document state themselves or write multiple files need separate review.

Full documentation is available at [Kajabity Tools](http://www.kajabity.com/kajabity-tools/).
