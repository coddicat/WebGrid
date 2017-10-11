# WebGrid
Strong typed ajax WebGrid uses razor and linq technologies

# Get started

### you need jquery.unobtrusive-ajax.min.js
### for example add it to bundles:
```csharp
bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery.unobtrusive-ajax.min.js",
                        "~/Scripts/jquery-{version}.js"));
```

### include js and css files 
```html
<head>
  <style type="text/css">
      @Solomonic.WebGrid.WebGrid.StyleSheet()
  </style>
  <script type="text/javascript">
      @Solomonic.WebGrid.WebGrid.JavaScripts()
  </script>
</head>
```

### Row Model Person.cs 
```csharp
public class Person
{
    public int Id { get; set; }
    [DisplayName("First Name")]
    public string FirstName { get; set; }
    [DisplayName("Last Name")]
    public string LastName { get; set; }
    public Gender Gender { get; set; }
}
```

### _WebGridView.cshtml
```csharp
@using Models
@using Solomonic.WebGrid
@using Solomonic.WebGrid.Models
@model GridModel<Person>
@{
    Model.Grid = Model.Grid ?? new WebGrid<Person>(x => x.Id.ToString());
    Model.Grid
        .AddColumn(new WebColumn<Person>(person => person.Id))
        .AddColumn(new WebColumn<Person>(person => person.FirstName))
        .AddColumn(new WebColumn<Person>(person => person.LastName))
        .AddColumn(new WebColumn<Person>(person => person.Gender));
    Model.ApplySettings();
}

@Html.For(x => x.Grid, x => x.WebGridSettings, true)
```

### Index.cshtml
```csharp
@using Models
<h2>Person List:</h2>
@(Solomonic.WebGrid.WebGrid.AjaxGrid<Person>(this, "formWebGrid", "SubmitWebGrid", "_WebGridView"))
@section scripts {
    <script type="text/javascript">
        $(function () {
            $('#formWebGrid').submit();
        })
    </script>
}
```

### HomeController.cs
```csharp
public ActionResult SubmitWebGrid(GridModel<Person> gridModel)
{
    gridModel.Grid = new WebGrid<Person>(x => x.Id.ToString())
    {
        Datas = _personQuery
    };
    return PartialView("_WebGridView", gridModel);
}
```
# Customization

### Column Title
```csharp
...
    .AddColumn(new WebColumn<Person>(person => person.FirstName)
    {
        Caption = "First Name of Person"
    })
...
```

### Data Value
```csharp
...
    .AddColumn(new WebColumn<Person>("Full Name")
    {
        DataValue = person => person.FirstName + " " + person.LastName
    })
...
```

### Column Width and Html Attributes

```csharp
...
  .AddColumn(new WebColumn<Person>(person => person.Id)
  {
      ColumnWidth = "100px",
      HtmlAttributes = new {style = "text-align: right", @class = "text-muted" }
  })
...
```

### Sortable
```csharp
//when DataValue is auto by property
  .AddColumn(new WebColumn<Person>(person => person.FirstName)
  {
      Sortable = true,
  })
  
//when Custom DataValue
  .AddColumn(new WebColumn<Person>("Full Name")
  {
      DataValue = person => person.FirstName + " " + person.LastName,
      Sortable = true,
      SortQuery = (query, descending) => descending 
          ? query.OrderByDescending(person => person.LastName).ThenByDescending(x=>x.FirstName) 
          : query.OrderBy(person => person.LastName).ThenBy(x=>x.LastName)
  })
```
