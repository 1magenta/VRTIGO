# MAPL-VRTIGO

## FingerTarget Test Iteration

### Overview

This document outlines the adjustments made to the FingerTarget test in response to the previous requirements for precise dysmetria measurement. Majors changes are made to FingerTarget task, the "Recenter Player" modification applied to UI in all tasks.

> **Navigation:** Scene `FingerTarget` → GameObject `SceneCode` → Component `FingerTargetNew.cs`

---

### Implemented Solutions

#### 1. Minimum 25 Reaches Per Arm

**Previous Issue:** Hardcoded trial counts with unclear structure made it difficult to modify reach quantities.

**Solution:**
- Converted hardcoded values to clearly named constants
- Set `REACHES_PER_ARM` to 25 (50 total recorded reaches)
- Total trials: 60 (10 practice + 50 recorded)
- Hand switch occurs automatically at trial 36

```csharp
private const int FIRST_RECORDED_TRIAL = 11;      // First trial after practice
private const int REACHES_PER_ARM = 25;           // Number of reaches per arm
private const int SWITCH_HANDS_TRIAL = 36;        // When to switch hands
private const int TOTAL_TRIALS = 60;              // Total trial count
```

> **To Modify (If needed):** Change `REACHES_PER_ARM` in the code if different quantities are needed.

---

#### 2. Fingertip Tracking for Precise Pointing

**Previous Issue:** System tracked palm position instead of fingertip.

**Solution:**
- Implemented direct index fingertip tracking
- Added visual debug system for verification

**Inspector Settings:**
| Setting | Description | Default |
|---------|-------------|---------|
| **Left/Right Index Tip Joint** | References to fingertip transforms | Pre-configured |
| **Show Fingertip Debug** | Toggle green debug sphere | OFF (keep off during tests) |

**Debug Indicators:**
- Green sphere = Direct fingertip tracking active
- Red sphere = Fallback mode (hand position + 8cm)
- Only enable "Show Fingertip Debug" when in need to debug. 

---

#### 3. Fingertip Trajectory Recording

**Previous Issue:** Only final hit position was recorded, only record the palm position instead of fingertip.

**Solution:**
- Continuous trajectory logging throughout reach movements
- Separate `FingertipTrajectory.txt` file with detailed movement data. (The original data log is not removed or overridden)
- Configurable sampling rate

**Inspector Settings:**
| Setting | Description | Default |
|---------|-------------|---------|
| **Trajectory Log Rate** | Samples per second | 30 Hz |

**Recorded Data:**
- Timestamp and trial information
- 3D position and velocity
- Distance to target
- Movement direction vector
- Head and eye positions

---

#### 4. Target Distance Calibration (Always too close)

**Previous Issue:** 
- Only measured Z-axis distance, ignoring lateral reach components
- Inaccurate distance calculation: `zDistance = handPos.z - headPos.z`

**Solution:**
- Full 3D Euclidean distance calculation
- Configurable distance adjustments
- Proper reach distance measurement from shoulder to fingertip

**Inspector Settings:**
| Setting | Description | Default | Recommended |
|---------|-------------|---------|-------------|
| **Target Distance Multiplier** | Percentage of full reach (0.9-1.0) | 1.00 | 1.00 |
| **Target Distance Offset** | Additional distance to subtract (meters) | 0.0 | 0.0 |

> **Note:** The pinch gesture used during calibration already provides a natural safety margin, so additional offset may not be necessary.

---

#### 5. Shoulder-Centered Target Positioning

**Previous Issue:** Targets spawned relative to VR headset, not the anatomical shoulder where arm movement originates.

**Solution:**
- Dynamic shoulder position estimation based on head location and handedness
- All calculations now originate from estimated shoulder position
- Researchers can configure offsets for different body types in the inspector before each user study session if needed. 

**Inspector Settings:**
| Setting | Description | Default |
|---------|-------------|---------|
| **Shoulder Drop From Head** | Vertical distance (meters) | 0.25 |
| **Shoulder Lateral Offset** | Horizontal distance (meters) | 0.18 |
| **Show Shoulder Debug** | Toggle shoulder position sphere | OFF |

**Debug Visualization:**
- Yellow sphere = Calibration phase
- Blue sphere = Normal operation
- Debug lines show reach vectors during calibration

---

#### 6. Hit Detection Accuracy Fix (New detected bug)

**Previous Issue:** Hit detection only checked Z-coordinate, allowing false positives without actual target contact.

**Solution:**
- Implemented 3D spherical hit detection
- Configurable detection radius

**Inspector Settings:**
| Setting | Description | Default |
|---------|-------------|---------|
| **Hit Detection Radius** | Detection sphere size (meters) | 0.03 |

---

#### 7. VR Recentering

**Previous Issue:** "Recenter Player" button was non-functional.

**Solution:** Fixed recentering in `RecenterOVR.cs`

**Important Warnings:**
- Recentering breaks Meta Quest Pro's spatial tracking, it may conflict with the build-in recenter, using both methods may lead to incorrect result. 
- **Avoid force to recenter if user already / familiar with using Meta's build-in recenter.**  
- Each recenter creates a new data logging folder
- Try to avoid using during the FingerTarget task if the recording starts to keep the data log well organized

---

### Quick Setup Checklist

- [ ] Verify index tip joints are assigned in Inspector
- [ ] Set appropriate shoulder offsets for participant's body type
- [ ] Adjust hit detection radius if needed (default: 0.03m)
- [ ] **Disable all debug options before participant testing**
- [ ] Avoid using both recenter methods during tasks

---

### Data Output

#### Files Generated - FingerTarget Task
- `TrialData.txt` - Summary of each reach attempt
- `FingertipTrajectory.txt` - Detailed movement trajectories
- `HeadPosition.txt`, `HeadRotation.txt` - Head tracking data
- `LeftEyeRotation.txt`, `RightEyeRotation.txt` - Eye tracking data
- `LeftHandPosition.txt`, `RightHandPosition.txt` - Hand position data

#### Folder Structure
```
VisInvisStability/
└── [PlayerName]/
    └── [DateTime]/
        ├── TrialData.txt
        ├── FingertipTrajectory.txt
        └── ... (other tracking files)
```