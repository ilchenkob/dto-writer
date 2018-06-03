## DTO Writer
### Extension for Visual Studio 2017

Simple code generation tool for DTO-classes creation.
You can easily create DTO-classes for your models in three clicks:
1. Make right mouse click in Solution Explorer on your .cs file that contains model classes.
2. Click "Create DTO" in context menu (You can find the same menu item in context menu of code editor area).
3. Click "OK" in creator dialog window.

This is it! Your DTO(s) will be generated, saved into new file and this file will be added to your project.

During DTO creation you can select which model properties should be skipped, do you need mapping methods or not, and change the location where the output file should be created. Optionally you can choose to add the following attributes:
- **JsonProperty** attribute from [Newtonsoft.Json nuget package](https://www.nuget.org/packages/Newtonsoft.Json/)
and / or
- **DataContract** + **DataMember** attributes from **System.Runtime.Serialization** assembly.

These attributes can be added later. You can find code fix suggestions for adding/removing attributes (they will be offered when cursor is placed at DTO class name or one of it's property).

It's already available at [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=VitaliiIlchenko.DtoCreator)

You can find **change log [here](https://github.com/ilchenkob/dto-writer/blob/master/CHANGELOG.md)**

#### Example

Source file content:

```csharp
using System;
using System.Collections.Generic;

namespace SampleApp.Models
{
  public class Person
  {
	public string FullName { get; set; }

	public int Age { get; set; }

	public DateTime Birthdate { get; set; }

	public string[] Skills { get; set; }

	public List<Person> Friends { get; set; }
  }
}
```

Output file content:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleApp.Models.Dto
{
    public class PersonDto
    {
        public string FullName { get; set; }

        public int Age { get; set; }

        public DateTime Birthdate { get; set; }

        public string[] Skills { get; set; }

        public List<PersonDto> Friends { get; set; }

        public static PersonDto FromModel(Person model)
        {
            return new PersonDto()
            {
                FullName = model.FullName, 
                Age = model.Age, 
                Birthdate = model.Birthdate, 
                Skills = model.Skills.ToArray(), 
                Friends = model.Friends.Select(PersonDto.FromModel).ToList(), 
            }; 
        }

        public Person ToModel()
        {
            return new Person()
            {
                FullName = FullName, 
                Age = Age, 
                Birthdate = Birthdate, 
                Skills = Skills.ToArray(), 
                Friends = Friends.Select(dto => dto.ToModel()).ToList(), 
            }; 
        }
    }
}
```