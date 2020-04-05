# Statiq Licensing FAQs

## How is Statiq licensed?

Statiq is licensed differently depending on the particular project:

- [Statiq Web](https://github.com/statiqdev/Statiq.Web) and other Statiq applications are dual-licensed under the [License Zero Prosperity Public License](https://licensezero.com/licenses/prosperity) (referred to below as the _Prosperity License_) and the [License Zero Private License](https://licensezero.com/licenses/private) (referred to below as the _Private License_). The Prosperity License limits commercial use to a 32 day trial period, after which a license fee must be paid to obtain a Private License. A [License Zero Waiver](https://licensezero.com/licenses/waiver) (referred to below as a _Waiver_) may also be granted waiving the non-commercial clause of the Prosperity License without requiring the purchase of a Private License. These are granted on a case-by-case basis to friends, family, contributors, folks who do good things in their community, and when the person requesting one makes a compelling case about why the non-commercial clause shouldn’t apply to them.
- [Statiq Framework](https://github.com/statiqdev/Statiq.Framework) and most other peripheral Statiq projects such as the [web site](https://github.com/statiqdev/statiqdev.github.io) are provided under a permissive MIT open-source license. This scheme is commonly referred to as "[open-core](https://en.wikipedia.org/wiki/Open-core_model)" because the core of the project is available as open-source while expanded capabilities are offered under more restrictive terms.

## How much is the license fee for the Private License?

The license fee for a Private License is currently $50 per developer, per major version. Obtaining a Private License for a particular major version of Statiq gives you the right to use that major version and all minor and patch versions after it for commercial purposes, forever, across all Statiq projects that are publicly licensed under the Prosperity License. In addition, if a new major version of Statiq is released within a one year grace period of purchasing a Private License, a new Private License covering the newly released major version will be provided. When a new major version of Statiq is released outside your one year grace period, you must obtain a new license to continue using it commercially.

**Note that only commercial use requires a license**. If you are not using Statiq Web or other Statiq projects licenses under the Prosperity License commercially you don't need a Private License or Waiver. Other than the non-commercial clause, the Prosperity License is essentially the same as any other permissive open-source license.

## I still have concerns about the vagueness and lack of legalese in the license.

The simplicity of the license is intentional. It uses [flipped form](https://flippedform.com/) in everyday English specifically to make it easier for laypeople to understand and agree to. An individual developer shouldn't have to enlist a lawyer every time they need to decide if they can use an open source project. Likewise, open source projects should be free to choose more sophisticated licenses without resulting in such legal review for users. Making all projects stick to traditional OSI/FSF licenses that cannot apply restrictions on use greatly limits the ability of projects to identify and use sustainability and funding strategies that work for them.

Here's the thing: I get it. The license is short and in the absence of lots of comforting legal jargon, how can it possibly be sound? If you don't agree that simple [flipped form](https://flippedform.com/) licenses provide the necessarily legal protections, how about this:

> I, David Glick (alias Dave Glick), hereby proclaim henceforth that no legal suit, pantsuit, or leisure suit shall be brought against any persons or animals engaged in the use of this software without exclusion, all jams and jellies preserved.

In short, **I won't sue you**. The license is primarily designed to express intent. The intent is that if you are commercially benefiting from the work that I've performed, I would like to share in that prosperity. If the language of the license combined with my assurances here does not provide you adequate legal protection, then I suggest you either purchase a Private License which removes all uncertainty and/or [make suggestions on how to improve the Prosperity Public License](https://github.com/licensezero/prosperity-public-license).

## What if I want to use Statiq on a server?

That's fine as long as all developers that created and provisioned everything have a license.

## What if I work for a non-profit?

The Prosperity License addresses that directly (one of the reasons I like it):

> Use by any charitable organization, educational institution, public research organization, public safety or health organization, environmental protection organization, or government institution doesn't count as use for a commercial purpose regardless of the source of funding or obligations resulting from the funding.

## Do content writers and other people that work on the inputs to the tool need licenses?

No. Unless the user is running Statiq directly, using Statiq Web in a library or application, or instructing a server how to run Statiq they do not need a license because they are not a "user" of the project. It's expected that at least one person in each entity subject to the non-commercial clause would require a license.

## What software does the Private License cover?

The Private License granted upon paying the license fee covers all software and projects under the Statiq organization that are licensed under the Prosperity License and require licensing. For example, it grants you the right to use both Statiq Web and Statiq Docs for commercial purposes. Statiq Framework, official themes, and other extensions are often licensed under more permissive terms and don't require a Private License in any case.

There are no other additional rights or privileges granted under the Private License. Specifically, it does not guarantee enhanced (or even any) support or priority issue resolution.

## You mean after I pay the license fee I don't get support?

That's correct. This is still a notionally open-source project and a small licensing fee does not cover an increased support burden. That said, I do my best to respond to every support request eventually (if not right away).

## If I purchase a license, do other users of the application I built with Statiq also require one?

No. In general the Private License includes provisions for sublicensing to your users as long as they don't then use the tools to create their own software.

## But this isn't actually open-source!

You're correct, at least for the "official" definition of open source. Both the [Free Software Foundation](https://www.gnu.org/philosophy/free-sw.en.html) and the [Open Source Initiative](https://opensource.org/osd-annotated) explicitly exclude software that contains limits on use from the definition of "open source". For this reason, the Prosperity License this project uses is not, and likely never will be, an OSI-approved license.

I understand that "free software" and "open source" generate a lot of feelings. If you prefer to call this project proprietary because of the way it's licensed, so be it. This is _"a proprietary project with freely available source code that can be used without restriction for non-commercial purposes and that engages with its community of users and accepts and encourages their contributions to source code, documentation, and support when they feel it's in their best interest to do so"_. Call that model whatever you want.

## Why not use a copyleft license instead?

A copyleft license places restrictions on consumers that enforce the "openness" of the software. This is great if your goal is to further the reach of the openness but does nothing to address the sustainability problem. It also places these restrictions indiscriminately - it doesn't matter if you're using the project for commercial or non-commercial uses. When tied to a dual-licensing model in which a less restrictive private license can be purchased, the incentive for doing so is connected to an assumption that licensees have an interest in using the software in a less open environment such as closed-source.

Rather than tie funding to whether or not the consumer wants to also make their code open, it makes more sense to me to tie _paying_ money _for_ use to _getting_ money _from_ use. In other words, _if you're generating revenue in part because of the work done on this project, then you should share some of that with the person who did the work you're using to generate the revenue_.

There's also a practical matter in that copyleft licenses only work well with libraries because the license deals with what happens to code that consumes or uses the copyleft-licensed software. Statiq is generally used in an application so it's not usually integrated with other code for distribution, making the provisions of a copyleft license less applicable.

## I want to contribute, why do you make me sign a CLA?

When an open source project uses a non-permissive license the question of how to deal with contributions comes up. A contributor license agreement (CLA) clarifies what will happen to the contributed code and requires the contributor to agree to it, waiving some of their own copyrights in the process. It's unfortunate and does add a burden to the contribution process but it's necessary to ensure the entire project stays properly licensed and contributions can be licensed under the terms of the Prosperity License _and_ the Private License. [Statiq's Contributor Agreement](https://gist.github.com/daveaglick/c7cccacdf7f3d57d05462a64d578d0a5) is designed to be as simple as possible while granting the broadest rights to the project under a non-exclusive agreement (it was developed using [Contributor Agreements](http://contributoragreements.org/) and then slightly modified).

I hope that you understand why a CLA is needed and that it doesn't keep you from contributing. That said, you need to decide if you derive enough value from submitting a contribution (such as adding support for a feature you need) to agree to the terms of the CLA. If not then I would caution you not to undertake performing work on the project.

## What about the contributions back when Statiq was Wyam?

Wyam was licensed under a permissive MIT license and so all contributions prior to the release of Statiq would have also fallen under that license. To put it another way, the MIT licensing of the project meant that anyone, including the project itself, could come along, take the code, and do whatever they wanted with it including re-licensing or charging for it. I'm more than happy to grant a Waiver to anyone who previously contributed to Wyam and would like to remove the non-commercial clause of the Prosperity License.
