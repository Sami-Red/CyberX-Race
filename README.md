# CyberX-Race
An **autonomous AI racing agent** built with **Unity ML-Agents**.  
Developed as my **final-year BSc Computer Game Development project**, where I trained an AI agent to drive around a custom racing track without human input.



## Project Overview
CyberX-Race explores how **reinforcement learning** can be applied to game AI.  
The agent was trained using **Unity ML-Agents Toolkit (PPO algorithm)** over 2 months to autonomously:
- Navigate a racing circuit
- Avoid collisions with walls
- Learn efficient racing lines over time

This project demonstrates how modern machine learning techniques can create **adaptive, self-learning game AI** beyond traditional scripted approaches.

## Preview

**YouTube Demo:**  
[![CyberX-Race Demo]](https://www.youtube.com/watch?v=zvTQrPHAR90)

**Itch.io Page (Coming Soon):**  
*(Link will be added here once live)*

## Features
- Custom Racing Environment built in Unity
- Physics-based Vehicle Controls (acceleration, braking, steering)
- Reward Shaping for progression, staying on track, and minimizing collisions
- Trained Autonomous Agent using ML-Agents PPO
- Visual debug tools for track progress & training feedback

## Project Structure

```
CyberX-Race/
├── Assets/              # Unity project assets (scenes, prefabs, scripts)
│   ├── Scripts/         # Car controller, agent logic, environment setup
│   ├── ML-Agents/       # Training configurations & brain settings
├── Models/              # Trained agent models (ONNX)
├── README.md            # Project documentation
```



## Training Process
- **Algorithm:** Proximal Policy Optimization (PPO)
- **Training Duration:** ~2 months of experimentation & tuning
- **Reward Function Design:**
  - Positive reward for forward progress along track
  - Penalty for collisions & going off track
  - Bonus for completing laps efficiently

Over time, the agent improved from random driving → staying on track → completing laps consistently.

---

## How to Run
1. Clone the repo:  
   ```bash
   git clone https://github.com/Sami-Red/CyberX-Race.git


2. Open the project in Unity 2021.x (with ML-Agents installed)
3. Load the racing scene
4. Press Play to watch the trained AI agent race!



## Technologies Used

* Unity 2021
* C# (Game Logic & Environment)
* Unity ML-Agents Toolkit (Reinforcement Learning)
* Python (Training Interface)



## Future Improvements

* Multi-agent racing (AI vs AI)
* Compare trained AI with scripted opponents
* Advanced reward functions for overtaking & racing lines
* Dynamic tracks & obstacles

