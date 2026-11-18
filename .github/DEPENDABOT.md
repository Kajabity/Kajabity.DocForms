# Dependabot auto-merge

The [auto-merge workflow](workflows/dependabot-automerge.yml) enables auto-merge for
minor and patch version updates in the `test-tooling`, `resource-support`, and
`actions-nonbreaking` groups defined in [dependabot.yml](dependabot.yml). Major
updates, ungrouped updates (including the pre-1.0 Kajabity.Tools packages), and
security update pull requests remain for manual review. The workflow uses verified
Dependabot metadata, so no repository labels are required.

When a Dependabot pull request receives a new commit, the workflow clears any
existing auto-merge setting and enables it again only if the updated pull request
still has verified Dependabot metadata and meets the group and version rules.
Pull requests with maintainer commits require manual review.

To activate this after the workflow reaches `main`, configure these GitHub settings:

1. Protect `main` with **Require status checks to pass before merging**. Require
   the build job from **Build and test** and the pull request jobs from
   **Security scans**. Otherwise GitHub may merge an eligible PR before CI finishes.
2. Under **Settings → General → Pull Requests**, enable **Allow auto-merge**.

Any required reviews still apply. The workflow does not approve pull requests.
