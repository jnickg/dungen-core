# DunGen Core
> _The .NET Core implementation of DunGen_

DunGen is framework powering the world's best random dungeon generator. Designed to be both easy-to-use, and highly customizable, and portable to any game system (both tabletop and desktop), it's the last dungeon generator you'll ever need.
## App Status
_NOTE: This is currently a work in progress. See our test site [here](https://dungen-core.herokuapp.com/)_

| Branch  | Build / UT  | Deployment |
|---|---|---|
| `master` | ![BuildAndUT](https://github.com/jnickg/dungen-core/workflows/Build%20and%20Unit%20Test/badge.svg) | ![Heroku](https://pyheroku-badge.herokuapp.com/?app=dungen-core&style=flat) |

# About the Framework
The DunGen framework consists of multiple components:

* A model for a grid-based "dungeon", including labels for its layout & anatomy, and all kinds of infestations.
* A powerful Algorithm design with a slew of smart boilerplate algorithms, plus a plugin interface to write new ones, and the ability to create your own composite algorithms from any of them.
* All kinds of terrain-generating algorithms, which creat the dungeon's layout and annotate the layout with labels (e.g. "this is room 1," or "this is the hallway leading to the boss")
* All kinds of infestation algorithms, which fill your dungeon with any of the denizens, traps, and phat loot that you can find in your Library.
* Powerful serialization, including the ability to dynamically reproduce an entire dungeon through its _algorithm runs_. This also means you can load a dungeon's _runs_, modify their parameters, and make a slightly _different_ dungeon (or the same one, with a different random seed).
* An ORM layer to interface with any major relational database (MySQL, Postgres, etc.), allowing you to create and share your own systems.

# Building & Developing
> _See [CONTRIBUTING.md](./CONTRIBUTING.md) for more information._

# Code of Conduct
> _We use the [Contributor Covenant](https://www.contributor-covenant.org/) v2.0._  
> _See [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md) for more information._

# License
> _We use the GNU General Public License, version 2._  
> _See [LICENSE.md](./LICENSE.md) for the full text._

# Support
> _Contact [jnickg](https://github.com/jnickg/) with questions, or to join the team._
