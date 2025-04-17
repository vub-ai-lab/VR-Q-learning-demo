# 🧠 VR Q-Learning Maze Demo

A VR-based interactive educational game demonstrating **reinforcement learning principles** using **Q-learning with eligibility traces** (Watkins’ Q(λ)). Developed as an engaging way to visualize how an agent learns optimal paths in a maze through experience and reward feedback.

---

## 🎮 Story & Setup

You are the RL agent.

Explore a **static maze** filled with treasure chests — only one contains treasure. From any random starting position, you must learn through trial, error, and feedback to find the shortest path to the treasure.

---

## 🧠 RL Concepts Illustrated

| Concept              | Description |
|----------------------|-------------|
| **State**            | Position in the maze |
| **Action**           | Move from current tile (up, down, left, right) |
| **Q-value**          | Expected utility of taking an action from a state |
| **Eligibility Traces** | Influence of recently visited state-action pairs (visualized as cones) |

---

## 🕹️ Gameplay Mechanics

- 🌀 The maze layout is fixed.
- 🔒 Some tiles have treasure chests (only one is correct).
- 🌫️ Maze visibility is limited (fog of war — only adjacent tiles visible).
- ⬛ Floor tiles indicate **Q-values** and **state-values**:
  - Green tiles = high value
  - Black tiles = low/zero value
- 🔺 Cones behind the player show the influence of eligibility traces.

### 🎯 Player Goals

- Navigate using a VR controller.
- Open treasure chests to find the goal.
- Learn optimal paths over multiple **episodes**.

---

## 🔁 Game Flow

### 🔄 New Episode

Triggered after treasure is found.

- Player is moved to a new random position.
- Q-values and tile colors update.
- Cones reset.

### 🆕 New Game

Triggered manually (to be implemented).

- Resets all Q-values, eligibility traces, and tile visuals.
- Player starts fresh.

---

## ⚙️ Controls & Parameters

| Control              | Description |
|----------------------|-------------|
| 📍 Move               | Navigate between adjacent tiles using VR remote |
| 🎲 Random Teleport   | Move to a random tile |
| ⚙️ Adjust Parameters | γ (discount), α (learning rate), λ (trace decay) |
| 🔄 New Game          | Reset everything and restart (coming soon) |

---

## 👓 Views

| View                  | Description |
|------------------------|-------------|
| **Player View**        | First-person immersive VR experience |
| **External View**      | Displayed on monitor: player view + top-down view of the maze and learning progress |

---

## 🔮 Planned Improvements

- [ ] Menu to choose maze layouts
- [ ] In-game guide / help screen
- [ ] Language selection at game start
- [ ] Multi-language support for UX

---

## 🤖 Under the Hood

### Reinforcement Learning Algorithm

**Q-learning with Eligibility Traces (Watkins’ Q(λ))**

- Efficient learning through temporal difference updates
- Eligibility traces allow recent state-action pairs to retain temporary influence

🔍 Check the source code for full implementation.

📚 Recommended reading:
> Sutton & Barto – *Reinforcement Learning: An Introduction*  
> [Available here](http://incompleteideas.net/book/RLbook2020.pdf)

---

## 📦 Project Setup

| Component     | Version     |
|---------------|-------------|
| Unity         | `2019.4.40f1` |
| SteamVR       | `v1.2.3` (deprecated) |
| VRTK          | `v3.x` |

### 🛠️ To Run:

1. Clone the repository:
   ```bash
   git clone https://github.com/vub-ai-lab/VR-Q-learning-demo.git
   cd VR-Q-learning-demo
