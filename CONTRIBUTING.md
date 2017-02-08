# How to contribute

Thanks for your interest in getting involved with Wyam! This project was always envisioned as a community effort. Any contributions you're able to make are **greatly appreciated**.

## Where To Start?

Here are a few ways you could get involved:

- Report a bug by opening an issue.
- Suggest or request a feature by opening an issue.
- Enhance or make corrections to the Wyam documentation.
- Write a new module and open a pull request for it.
- Write a blog post about your experiences and let us know about it.

If you need help with any of these things, **please don't hesitate to ask**. Wyam is a beginner-friendly project and we want to help anyone contribute who would like to.

Build instructions are in the `BUILDING.md` file at the root of the repository.

## Submitting Code

If you would like to submit code to Wyam, please follow the guidelines below. If your change is small, go ahead and submit a pull request and any questions can be discussed in the request. If it's a larger change, you should probably open an issue first so that it can be discussed before you start spending time coding.

Also note that most code contributions are going to be related to module development. If that's the case for your contribution, then please read [this article on module development](https://wyam.io/docs/extensibility/writing-a-module) first.

### Making Changes

[Fork](http://help.github.com/forking/) the Wyam repository on GitHub. Please do your work in feature branches based on `develop`. `develop` is the main development branch and is where all code is pushed before making a release.

### Handling Updates from Upstream/Master

While you're working away in your branch it's quite possible that your upstream master (most likely the canonical Wyam repository) may be updated. If this happens you should rebase your local branch to pull in the changes. If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

### Sending a Pull Request

While working on your feature you may well create several branches, which is fine, but before you send a pull request you should ensure that you have rebased back to a single feature branch. When you're ready to go you should confirm that you are up to date and rebased with upstream/master (see above).

### Style Guidelines

Wyam generally follows accepted .NET coding styles (see the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042%28v=vs.110%29.aspx)). Please note the following differences or points:

- Indent with 4 spaces, not tabs.
- Prefix member names with an underscore (`_`).
- Try to use explicit type names unless the type is extremly long (as can happen with generics) or overly complex, in which case the use of `var` is acceptable.
- Use the C# type aliases for types that have them, e.g. `int` instead of `Int32`, `string` instead of `String` etc.
- Use meaningful names (regardless of length) and try not to use abbreviations in your type names.
- Wrap `if`, `else` and `using` blocks (or blocks in general, really) in curly braces, even if it's a single line. The open and closing braces should go on their own line.
- Pay attention to whitespace and extra blank lines.
- Be explicit with access modifiers. If a class member is private, add the `private` access modifier even though it's implied. 
- Avoid `#region`. There is debate on whether regions are valuable, but one of the perks of being a benevolent dictator is that I can restrict their use in this code.
- Constants should be all uppercase and should be placed at the top of their containing class.

### Dependencies

Generally, you should only ever have to take a dependency on `Wyam.Common`. If you find yourself needing to depend on other Wyam libraries like `Wyam.Core`, please open an issue so the needed functionality can be moved into the common assembly.

### Unit Tests

Make sure to run all unit tests before creating a pull request. You code should also have reasonable unit test coverage.

## Updating Documentation

Making updates to the Wyam documentation is just as helpful as writing code (if not more helpful). The Wyam documentation exists in the [Wyam.Web repository](https://github.com/Wyamio/Wyam.Web). Fork and clone the repository as you would the main code repository, but make any changes in the `master` branch. Submit your changes as a pull request. Once it's accepted, the site will automatically rebuild and redeploy. 

## License
By contributing to Wyam, you assert that:

* The contribution is your own original work.
* You have the right to assign the copyright for the work (it is not owned by your employer, or
  you have been given copyright assignment in writing).
