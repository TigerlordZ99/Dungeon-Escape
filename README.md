# Dungeon Escape

A procedurally generated roguelike dungeon crawler built in Unity with C#.

**[Play the game here](https://tigerlordz99.github.io/Dungeon-Escape/)**

---

## Overview

Dungeon Escape is a top-down 2D roguelike where players navigate a randomly generated dungeon, collecting color-coded keys to unlock doors and find the Master Key to escape. Enemies spawn and scale in difficulty as you collect more keys, building tension throughout every run. No two runs are the same.

---

## Gameplay

- Use **WASD** to move
- Collect color-coded keys and use them to unlock matching colored doors
- Find the **Master Key** to unlock the final room and escape
- Avoid enemies — they spawn ahead of you and increase in frequency as you collect more keys
- Your health depletes on enemy contact — reach zero and it's over

---

## Procedural Systems

The dungeon is generated at runtime using a spine-based graph system, with a randomized room count between 6 and 15 and randomly branching side paths. A shuffled color-coded key chain is built each run, ensuring a unique and always-winnable progression path. Keys are placed inside rooms using tilemap scanning and `Physics2D.OverlapCircle` to ensure they only spawn on walkable tiles. Enemies spawn dynamically based on keys collected, using dot product calculations to place them in front of the player's movement direction.

---

## Built With

Unity · C# · WebGL · GitHub Pages

---

## Credits

**Developed by** Anish Bansal & Madhav Ramakrishnan

**Dungeon Layout Generator** by Noah Baron  
**Noise Based Random Item Generator** by Inho Yoo  
**BGM** by Shumworld on Freesound  
**SFX** by Freesound-community on Pixabay, Raclure and BradWesson on Freesound  
**Key sprite** by Giuseppe Ramos on Vecteezy  
**Enemy sprite** by Warren Clark on itch.io  
**Player sprite** by Pipoya on itch.io  

*AI Disclaimer: Generative AI coding tools such as Claude and ChatGPT were used to help develop this project.*
