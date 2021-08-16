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

### Filtering
#### Textbox
```csharp
...
  .AddColumn(new WebColumn<Person>(person => person.FirstName)
  {
      FilterFormat = column => WebGridHelper.FilterTextBox(column, Model.WebGridSettings, new { @class = "form-control" }),
      FilterQuery = (query, searchString) => query.Where(x => x.FirstName.Contains(searchString))
  })
...
```
#### Dropdown
```csharp
///key for FilterQuery and value for display
var genderDictionary = new Dictionary<string, string> { { Gender.Male.ToString(), "Man" }, { Gender.Female.ToString(), "Woman" } };
Model.Grid
    .AddColumn(new WebColumn<Person>(person => person.Gender)
    {
        FilterFormat = column => WebGridHelper.FilterDropDown(column, Model.WebGridSettings, genderDictionary),
        FilterQuery = (query, selectedKey) =>
        {
            Gender gender;
            return Enum.TryParse(selectedKey, out gender) ? query.Where(x => x.Gender == gender) : query;
        }
    });
```

### Custom Header and Cell Display
```csharp
@{
...
   .AddColumn(new WebColumn<Person>("actions")
   {
      CellFormat = cell => Actions(cell),
      CaptionFormat = CustomHeader()
   });
...
}

@helper Actions(WebCell<Person> cell)
{
    var personId = cell.Data.Id;
    <div>
        <button class="btn btn-sm btn-primary" onclick="editPerson(@personId)">
            <span class="glyphicon glyphicon-edit"></span> 
            Edit
        </button>
        <button class="btn btn-sm btn-danger" onclick="removePerson(@personId)">
            <span class="glyphicon glyphicon-remove"></span> 
            Remove
        </button>
    </div>
}

@helper CustomHeader()
{
    <span class="glyphicon glyphicon-wrench"></span>
}
```

## Row selection and Edit data in rows
### Row selection
#### _WebGridView.cshtml
```csharp
...
  .AddColumn(WebGridHelper.SelectRowColumn<Person>())
...
  <button name="SubmitSelectedRows">Submit Selected Rows</button>
  @Html.For(x => x.Grid, x => x.WebGridSettings, true)
...
```
#### HomeController.cs
```csharp
public ActionResult SubmitWebGrid(GridModel<Person> gridModel)
{
    if(gridModel.Rows != null)
    {
        var selectedRowsById = gridModel.Rows.Where(x => x.Checked).Select(x => x.Key).ToArray();
        //to do 
    }
    gridModel.Grid = new WebGrid<Person>(x => x.Id.ToString())
    {
        Datas = _personQuery
    };
    return PartialView("_WebGridView", gridModel);
}
```

### Edit data in rows
#### _WebGridView.cshtml
```csharp
...
  Model.Grid.SaveChangesButton = true; //show saving button on the header 
  var genderDictionary = new Dictionary<string, string> { { Gender.Male.ToString(), "Man" }, { Gender.Female.ToString(), "Woman" } };

  Model.Grid
      .AddColumn(new WebColumn<Person>(person => person.Gender)
      {
          CellFormat = cell => WebGridHelper.CellDropDown(cell, genderDictionary, (person, keyValue) =>
          {
              Gender gender;
              return Enum.TryParse(keyValue.Key, out gender) ? person.Gender == gender : false;
          }, new { @class = "form-control" })
      })
      .AddColumn(new WebColumn<Person>(person => person.FirstName)
      {
          CellFormat = cell => WebGridHelper.CellTextBox(cell, new { @class = "form-control" }),
      })      
...
```
#### HomeController.cs
```csharp
public ActionResult SubmitWebGrid(GridModel<Person> gridModel)
{
    if(gridModel.SaveChanges)
    {
        var changedRows = gridModel.Rows.Where(x => x.Changed);
        foreach(WebRow row in changedRows)
        {
            int personId;
            if(int.TryParse(row.Key, out personId))
            {
                Person person = PersonQuery.Single(x => x.Id == personId);
                person.FirstName = row.ColumnValues["FirstName"];

                Gender newGender;
                if(Enum.TryParse(row.ColumnValues["Gender"], out newGender))
                {
                    person.Gender = newGender;
                }
            }
        }
        //Db SaveChanges
    }
    
    gridModel.Grid = new WebGrid<Person>(x => x.Id.ToString())
    {
        Datas = _personQuery
    };
    return PartialView("_WebGridView", gridModel);
}
```
