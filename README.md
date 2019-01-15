# Welcome to CommunityKit
My name is Zane, and I'm working on a modern engine for games in the style of [The Shadow of Yserbius](https://en.wikipedia.org/wiki/The_Shadow_of_Yserbius). Thanks for checking it out!

Released in 1993, Yserbius was one of the first online graphical RPGs. Up to 60 people at a time could meet up, create adventuring parties, and delve deep into monster-filled dunegons. When you were done adventuring, you could venture off with your friends to play other games on the Imagination Network, which modestly described itself as "the friendlest place in cyberspace".

I have very fond memories of Yserbius and the Imagination Network. CommunityKit is my attempt to recreate that kind of experience: the ability to create a persona, to play fun games with other people, and to be a part of a community that is welcoming, helpful, and supportive. 

The proof-of-concept goal of CommunityKit will be a modern reimplementation of The Shadow of Yserbius. After the dungeon crawl is complete, my stretch goals are to add community features such as in-game mail, and add other common games such as board/card games. Following the completion of the stretch goals, I plan to use the dungeon crawn engine to build a brand new scenario, with new assets and mechanics.

This is the public repository for documentation and issue tracking for CommunityKit.
* Check out [issue tracking here](https://github.com/ZaneDubya/CommunityKitPublic/issues).
* Check out [progress on the current milestone here](https://github.com/ZaneDubya/CommunityKitPublic/milestone/4).

## Road Map (Estimated)
* Feature Complete: Dungeon (January 1, 2019), Tavern (January 15, 2019).
* Bug fixes, polish, and assets expected complete by end of January 2019.
* Additional polish and closed testing expected throughout Febuary 2019.
* Public replease expected early to mid March 2019.
* Hypercare and stabilization after launch expected to end May 2019.

## Development History
* As of January 15, 2019, I have been working on CommunityKit for about five and a half months.
* The first push was between April 17 and July 24, 2018.
* The current push began November 5, 2018 and is ongoing.
* My best estimate is that it will be ready for release in early to mid March 2019.
* I also expect it will need a rigorous bug testing period ("hypercare and stabilization") after release.

CommunityKit is written in C#.
* As of January 15, 2019, CommunityKit is 82,206 lines of code. 
  * The Server is 40,302 lines of code.
  * The Client is 36,101 lines of code.
  * The Shared Codebase is 5,803 lines of code.
* The server runs on .NET Core 2.0.
* The client runs on the XNA platform, portable to MonoGame/FNA for cross-platform releases.
