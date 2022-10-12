# Self Driving Vehicle

Let's say you are standing somewhere in a room and would like to find the shortest path to a goal. You can see a few obstacles, such as a table, that you would like to avoid. The easiest way to solve the problem (if you are a computer) is to divide the room into many small squares (cells) and then use the common A* (A Star) search algorithm to find the shortest path. 

But what if you are a car and can't turn around 360 degrees like a human can, then you have a problem! Well, at least until you learn the Hybrid A Star search algorithm. With that algorithm you will be able to find a fully drivable path to the goal!

Click for YouTube video of the algorithm in action:

[![Link to youtube video](https://img.youtube.com/vi/L591fS51F4I/0.jpg)](https://www.youtube.com/watch?v=L591fS51F4I)

If you just want to play around with it you can download a build of the project here for Windows: https://habrador.itch.io/hybrid-a-star



## Tell me how the algorithm works

You can read more about it here: https://blog.habrador.com/2015/11/explaining-hybrid-star-pathfinding.html



## Is this something actually being used by car companies?

Yes! Tesla mentioned the algorithm in a [Tesla AI Day](https://www.youtube.com/watch?v=j0z4FweCy4M) presentation (roughly at 1 hour 20 minutes). So if you ever wondered how the Tesla "Smart Summon" feature works then now you know! Tesla has included a short description of the Smart Summon feature (which is part of the Full Self-Driving Capability (FSD) version of Tesla Autopilot) in the [Model Y Manual](https://www.tesla.com/ownersmanual/modely/en_eu/GUID-6B9A1AEA-579C-400E-A7A6-E4916BCD5DED.html). We can assume it's the same implementation for other Tesla models, such as Model S. 

* **"Smart Summon works with the Tesla mobile app when your phone is located within approximately 6 meters of Model Y."** My implementation works over distances of greater than 6 meters. 

* **"Smart Summon may not stop for all objects (especially very low objects such as some curbs, or very high objects such as a shelf) and may not react to all traffic. Smart Summon does not recognize the direction of traffic, does not navigate around empty parking spaces, and may not anticipate crossing traffic."** My implementation has fixed obstacles only, and they all have the same height. I actually planned to add moving objects and traffic lanes with direction, but will not do so because Tesla's implementation can't handle them.

* **"Touch the crosshair icon then drag the map to position the pin on a chosen destination. Press and hold the GO TO TARGET button. Model Y moves to the destination."** My implementation is not just moving to a destination, but also with a specific target direction, such as the left door ends up infront of you.         



## FAQ 

* **What software do I need?** To make this project work you need [Unity](https://unity.com/). I've used Unity 2017-2021 but other versions should work as well. 

* **Is it working on a navmesh?** No, it's not! The algorithm needs a grid with cells to be able to remove unnecessary nodes, or you will end up with an infinite amount of nodes.



## TODO

* The car can follow the generated paths with great accuracy, but the truck with trailer is not that good at following the path. That has to be fixed!

