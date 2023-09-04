# Freecam
RedLoader ships with it's own configurable freecam mode that also adds smoothing.

### How to enable it
Press `F1` to open the console and type `xfreecam`.  
To exit the freecam mode, press `F1` and type `xfreecam` again.

### Configuration
You can configure various parameters of the freecam mode in the `UserData/_RedLoader.cfg` file.  

| Parameter              | Default Value | Description                                              |
|------------------------|---------------|----------------------------------------------------------|
| `look_sensitivity`     | `0.2`         | The mouse sensitivity multiplier when looking around     |
| `positional_smoothing` | `0.5`         | The amount of smoothing applied to the camera's position |
| `rotational_smoothing` | `0.01`        | The amount of smoothing applied to the camera's rotation |
| `mouse_y_ratio`        | `0.7`         | The ratio of the mouse's Y speed to the X axis           |

### Controls
- `WASD` to move around
- `Space` to move up
- `Right mouse` to move down
- `Mouse` to look around
- `Mouse Wheel` to change positional speed
- `Ctrl` + `Mouse Wheel` to change rotational speed