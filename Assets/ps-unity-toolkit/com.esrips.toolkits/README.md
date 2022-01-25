This package provides utility functions for:
- Feature Service:
  - RequestFeatures: Requests features from a feature service.

```
    var results = await FeatureService.RequestFeatures(whereClause)
    This function returns JObject of the feature request results
    The client can iterate through the JObject hierarchy

    var features = results["features"].Children();
    foreach (var f in features)
    {
        var attributes = f.SelectToken("attributes");
        var name = attributes.SelectToken("name").ToString();
        var geometry = f.SelectToken("geometry");
        float pos_x = 0.0f;
        float.TryParse(attributes.SelectToken("x").ToString(), out pos_x);
        float pos_y = 0.0f;
        float.TryParse(attributes.SelectToken("y").ToString(), out pos_y);
        float pos_z = 0.0f;
        float.TryParse(attributes.SelectToken("z").ToString(), out pos_z);
    }

```
- 
    - UpdateFeatures: Update features on a feature service.
   
```
    var results = await FeatureService.RequestFeatures(whereClause)
    This function returns JObject of the feature request results.
    The client can iterate through the JObject hierarchy.
    
    var features = results["features"].Children();
    foreach (var f in features)
    {
        var attributes = f.SelectToken("attributes");
        var name = attributes.SelectToken("name").ToString();
        var geometry = f.SelectToken("geometry");
        float pos_x = 0.0f;
        float.TryParse(attributes.SelectToken("x").ToString(), out pos_x);
        float pos_y = 0.0f;
        float.TryParse(attributes.SelectToken("y").ToString(), out pos_y);
        float pos_z = 0.0f;
        float.TryParse(attributes.SelectToken("z").ToString(), out pos_z);
    }

```
- Route Service:
  - RequestRoute: Requests route from a route service.

```
    var results = await RouteService.RequestRoute(origin, destination)
    This function returns JObject of the route request result
    The client can iterate through the JObject hierarchy
    
    var features = results["routes"].SelectToken("features");
    
    foreach (var f in features)
    {
        var attributes = f["attributes"];
        var geometry = f["geometry"];
        var paths = geometry["paths"];
        foreach (var path in paths)
        {
            var count = 0;
            foreach (var point in path)
            {
                //Debug.Log(point);
                var x = double.Parse(point[0].ToString());
                var y = double.Parse(point[1].ToString());
                count++;
            }
        }
    }

```
Geometry Helper Functions:
- CreatePlane: Creates a game object with plane geometry using width and height.
- CreatePlane: Creates a game object with plane geometry from 2 points.
- CreatePlane: Creates a game object with plane geometry from an array of vertices.
