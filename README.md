# Self-driving-vehicle

Let's say you are standing somewhere in a room and would like to find the shortest path to a goal. You can see a few obstacles, such as a table, that you would like to avoid. The easiest way to solve the problem (if you are a computer) is to divide the room into many small squares (cells) and then use the common A* (A Star) search algorithm to find the shortest path. 

But what if you are a car and can't turn around 360 degrees like a human can, then you have a problem! Well, at least until you learn the Hybrid A Star search algorithm. With that algorithm you will be able to find a fully drivable path to the goal!

Click for YouTube video of the algorithm in action:

[![Link to youtube video](https://img.youtube.com/vi/L591fS51F4I/0.jpg)](https://www.youtube.com/watch?v=L591fS51F4I)

If you just want to play around with it you can download a build of the project here for Windows: https://habrador.itch.io/hybrid-a-star



## Tell how the algorithm works

You can read more about it here: https://blog.habrador.com/2015/11/explaining-hybrid-star-pathfinding.html



## FAQ

* **Is this something actually being used by car companies?** Yes! Tesla mentioned the algorithm in a [Tesla AI Day](https://www.youtube.com/watch?v=j0z4FweCy4M) presentation (roughly at 1 hour 20 minutes). So if you ever wondered how the Tesla "Smart Summon" feature works then now you know! 

* **What software do I need?** To make this project work you need Unity. I've used Unity 2017 and 2018 but other versions should work as well. 

* **Is it working on a navmesh?** No, it's not! The algorithm needs a grid with cells to be able to remove unnecessary nodes, or you will end up with an infinite amount of nodes.



## TODO

* Upgrade to latest Unity version
* The car can follow the generated paths with great accuracy, but the truck with trailer is not that good at following the path. That has to be fixed!

