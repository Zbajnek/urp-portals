# Unity URP Portals
A port of [Sebastian Lague's portals](https://github.com/SebLague/Portals) into Universal Render Pipeline.

## Requirements
- URP v17

## Features
- Seamless portal rendering
- Recursive portal rendering
- Physical traversal - player and objects can pass through portals

## Installation
- Install via UPM git URL <br>
  `https://github.com/Zbajnek/urp-portals.git?path=Packages/URPPortals`

## Usage

### Basic Usage

1. Add `PortalRenderer.cs` onto your main camera.
2. Create portal objects
   - Option A: Use the portal prefab
      - Simply drag the two Portal prefabs into your scene and link them together <br>
   - Option B: Build your own portal
       - Create a portal prefab with the following structure:
       ```
         Portal
         |
         +-- Screen
         |
         +-- PortalCamera
       ```
       - For each portal:
         1. Add `Portal.cs` to the root GameObject
         2. Add a Collider (set as trigger) to the root GameObject and set its size to be bigger than the screen
         3. Add a Collider to the screen (set as trigger)
         4. Assign the Portal Material (using the `URPPortals/PortalMask` shader) to the screen
         5. Link the portals together

### Advanced Usage

To make any object be able to pass through portals, simply add the `PortalTraveller.cs` script to it and make sure
that it has a collider.

#### Extending PortalTraveller

For creating custom portal travellers, (like objects with physics-based movement), you can create your own traveller class:

```csharp
public class MyPortalTraveller : PortalTraveller 
{
    private Rigidbody _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    public override void Teleport(Vector3 pos, Quaternion rot)
    {
        _rb.isKinematic = true;
        
        base.Teleport(pos, rot);
        
        _rb.isKinematic = false;
    }
}
```

## TODO
- [ ] Slicing object with a slice shader

## Known Issues
- Traversing through portals while having SSAO enabled can cause graphical artifacts

## License
The package is under the MIT license. See [LICENSE](LICENSE) for details.
