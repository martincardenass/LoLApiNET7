# LoLApiNET7
Simple API to get League of legends champions.

For more information, visit the League of Legends Universe NextJS Front end repository
##### This project was inspired by the original League of Legends Universe

API for managing League of Legends champions, user accounts, authentication with JWT token, user reviews for each champions and other features.

## Table of contents
- [Features](#features)
- [Technologies used](#technologies-used)

## Features
### Champions
**Get the champions:** there are a total of 154 champions in the database. Of which, we store its name, region, role, icon, additional images, a catchphrase of them, and a brief description.

**Update the champions:**: every field of the champions can be updated. We take a JSON payload and only update the fields that have changed.
```
public interface IChampionService
{
  bool UpdateChampion(int Region_id, int Role_id, Champion champion);
}

public bool UpdateChampion(int Region_id, int Role_id, Champion champion)
{
  var existingChampion = _context.Champions.Find(champion.Champion_id);
    if (existingChampion == null)
    return false;

  existingChampion.Region_id = Region_id; // < set the regionId from query
  existingChampion.Role_id = Role_id; // < set the role id from query

  _context.Update(existingChampion);
  return Save();
}

```
In the controller, we map each field that will be updated, and we do some validations to ensure we receive the information that we need.
```
public IActionResult UpdateChampion(int champId, [FromQuery] int RegionId, [FromQuery] int RoleId, [FromBody] ChampionDto updatedChampion)
{ // dont forget to pass champion_id in the send body, otherwise it will not work
  if (!_championService.ChampionIdExists(champId)) //checking if the champId from query exists
  {
    var errorMsg = "A champion with Id: " + champId + " Does not exist"; // could use string interpolation here
    return NotFound(new { Message = errorMsg });
  }

  if (!_regionService.RegionIdExists(RegionId)) //checking if the RegionId from query exists
  {
    var errorMsg = "A region with Id: " + RegionId + " Does not exist"; // could use string interpolation here
    return NotFound(new { Message = errorMsg });
  }

  if (!_roleService.RoleIdExists(RoleId)) //checking if the RoleId from query exists
  {
    var errorMsg = "A role with Id: " + RoleId + " Does not exist"; // could use string interpolation here
    return NotFound(new { Message = errorMsg });
  }
            
  if (champId != updatedChampion.Champion_id) return BadRequest(); // if the ids from query and body dont match, return bad request

  var championMap = _championService.GetChampionById(champId); //getting the original value of the champion before editing it

  championMap.Name = updatedChampion.Name ?? championMap.Name; // Validating each field. If no value is provided, it wont be updated.
  championMap.Image = updatedChampion.Image ?? championMap.Image;
  championMap.Release_date = updatedChampion.Release_date ?? championMap.Release_date;

  if (!_championService.UpdateChampion(RegionId, RoleId, championMap))
  {
    ModelState.AddModelError("", "Something went wrong while updating the champion");
    return StatusCode(500, ModelState);
  }

  return NoContent();
}
```

**Delete a champion:** any champion can be deleted.
```
public interface IChampionService
{
  bool DeleteChampion(Champion champion);
}
```

We simply pass the champion object to delete
```
public bool DeleteChampion(Champion champion)
{
  _context.Remove(champion);
  return Save();
}
```

### Regions

**Get the regions:** there are a total of 14 regions in the database. Of which we store the name, description, emblemn and an image

And, just as the champions, its possible to update and delete them.

### Roles
**Get the roles:** there are 6 roles in the database. We store their name and icon

Just as champions and regios, we can update and delete them if we want to.

## Reviews

**Post Reviews:** Registered and logged in users can post reviews of any champion. A review needs to have a title, some text content, when it was created *(which will populate automatically when the review is posted)* and a rating from 1 to 5.
The process of posting a review consists of:
```
public bool CreateReview(byte Rating, int ChampionId, Review review)
{
  if (string.IsNullOrEmpty(review.Title)) //If no title for the review is provided
  {
    string words = review.Text[..16];  //Gets the first 16 words
    review.Title = words + "..."; //asign the to the title if no title is provided
  }

  var reviewInsert = new Review()
  {
    Rating = Rating, // Assign the rating. 0 - 5 TINYINT
    User_Id = _userService.DecodeToken(_userService.GetToken()), //Gets the user directly from the bearer token
    Champion_id = ChampionId, // Assign the champion who is being reviewed
    Title = review.Title,
    Text = review.Text,
    Created = DateTime.Now, // Data of creation will be current date
  };

  _context.Add(reviewInsert);

  return Save();
}
```
Of the code above, we
* We check if the title from the JSON payload its empty, if it is, it takes the first 16 characters of the text field from the JSON and sets the title to its value. The text field cannot be empty.
* Create a new instance of the Review model, mapping the values to pass it before adding it to database.
* The rating its gotten from the query, only takes values from 1 to 5.
* We get the user ID from the decoded token

To get the token, we need to use dependecy injection and use the IHttpContextAccessor interface

```
public UserService(AppDbContext context, IConfiguration config, IHttpContextAccessor accesor) 
{
  _context = context;
  _config = config;
  _accesor = accesor;
}
```

With that interface injected, we can now use it to access the authorization headers from the request. We will extract the JWT Bearer token from them:

```
public string GetToken()
{
  string bearerToken = _accesor.HttpContext.Request.Headers.Authorization.ToString(); // Get the token from the Authorization Headers
  bearerToken = bearerToken.Replace("Bearer", "").Trim(); // Its gote some characters that we dont need. We remove them

  return bearerToken; // Return the token
}
```

Then, we call our method DecodeToken which will take the token string that GetToken returned, and extract the UserId
```
public int DecodeToken(string token)
{ // Decodes the token and gets the User Id
  JwtSecurityTokenHandler tokenHandler = new(); // create the token handler
  JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token); // read the token

  //Create a userIdString that gets the value of the ID stored in the token.
  string userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

  //To be able to use it. We convert it to an integer
  _ = int.TryParse(userIdString, out int userIdInt);

  //and we return the integer.
  return userIdInt;
}
```
With that, we have specified which user its posting the review.
* We get the champion ID from the query, just as the rating
* The review.title and review.text are both gotten from strings in the JSON Payload
* Finally, the Created field will be the current date and time.

**Update reviews:** to update a review, we first need to check if the userId from the authorization token matches the review that they are trying to modify, because if it does not match, no changes can be made.
```
public bool UpdateReview(int ReviewId, byte NewRating, Review review)
{
  var reviewToUpdate = _context.Reviews.Find(ReviewId); // Find the review were going to update

  if (CompareUserIds(ReviewId) == true || _userService.IsUserAdmin()) // If the comparasion between the Ids results false
  {
    reviewToUpdate.Rating = NewRating; // Set the new rating

    _context.Update(reviewToUpdate);
    return Save();
  }
  else
    return false; // Return false which will cause a server error
}
```
*Even if a user manipulates the token so the id now matches another review, it wont be possible to update the review. Thanks to the secret key in our .env file*

The CompareUserIds method will return true if the userId from the token and the userId from the review we are passing matches, if not, it will return false. This is useful to make sure only the author of the review can modify it.
```
public bool CompareUserIds(int ReviewId)
{
  var reviewToUpdate = _context.Reviews.Find(ReviewId); // Find the review were going to update

  var userIdFromToken = _userService.DecodeToken(_userService.GetToken());
  var userIdFromReview = reviewToUpdate.User_Id;

  if (userIdFromReview != userIdFromToken) // If the userId contained in the review does not match the userid contained in the token
    return false; // return false, which will cause a server error because its expecting true
  else
    return true; //If they do match, return true
}
```

Additionally, admin users can modify or delete any review, no mather who posted them.
```
public bool IsUserAdmin()
{
  string userRoleFromToken = DecodeTokenRole(GetToken()).ToString();

  if (userRoleFromToken == "admin")
    return true; // User is an admin
  else
    return false; // User its not an admin
}
```

For this, we need an additional method that will decode once again the token, but will now return a string with the role from the token. Either "user" or "admin"
```
public string DecodeTokenRole(string token)
{
  JwtSecurityTokenHandler tokenHandler = new(); // Creates a new instance of the jwtsectokenhandl class
  JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

  // Create a string that gets the value of the ROLE stored in the token
  string roleString = jwtToken.Claims.FirstOrDefault(r => r.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

  // In this case we return a string
  return roleString;
}
```

If any of the two coniditions are met (matching ID or user admin), they can freely modify or delete the desired review.

**Get reviews by champion**: filter the review that have a certain champion id for easy access.

## Users
**Create user:** we can create a user just as follows, we need the NuGet package bcrypt to safely store an encrypted password in the database
```
public bool CreateUser(User user)
{
  string passwordHashed = BCrypt.Net.BCrypt.HashPassword(user.Password); //Encrypt the user's password

  var userInsert = new User()
  {
    Username = user.Username.ToLower(),
    Created = DateTime.Now,
    Email = user.Email.ToLower(),
    Role = user.Role.ToLower(),
    Password = passwordHashed
  };

  _context.Add(userInsert);
  return Save();
}
```

**Delete user:** just like reviews, users can be also deleted
```
public bool DeleteUser(User user)
{
  if (ComparedUserIds(user.User_Id) || IsUserAdmin()) // Admins can delete any user. But they can delete other admins. Will fix that later
  {
    _context.Remove(user); // If User Ids are the same, delete
    return Save(); // And save
  }
    else
    return false; // If not, return false and get server error
}
```

## Images collection

Champions have the ability to contain multiple images, to achieve this, we need three tables: champions, images, and championsimage.

* Champions will only store information of the champion.
* Images will only store images
* ChampionsImage will store the relation between champions and images. For example, champion with the ID of 3 will have images with id 1323, 1324 and 1325.

![](https://i.imgur.com/cWyfqw8.png)

Implementing this, we can easly have champions with multiple images.

**Adding images:** the process of adding an image will add the image to the images table, and will also save the relation to the ChampionsImage table, as follows:
```
public bool CreateImage(int ChampionId, Images image)
{
  _context.Add(image); // Add the image to the image table
  _context.SaveChanges(); // Save the changes otherwise we get an error
            
  var relation = new ChampionsImage() //This is so we know the relationship of the just uploaded image with the championId
  {
    Champion_Id = ChampionId,
    Image_Id = image.Image_Id
  };

  _context.Add(relation);  // Add the relationship to the relations table
  return Save();
}
```

**Deleting images"** will first delete the relationship and then the image
```
public bool DeleteImage(Images image)
{
  _context.Remove(_cIService.GetRelationByImageId(image.Image_Id)); // Deletes the relation
            
  _context.SaveChanges(); // Save the changes otherwise we get an error

  _context.Remove(image); // Deletes the actual image from the images table
  return Save();
}
```

## Using SQL views to make our life easier

Instead of doing SQL querys directly in the code, I have opted in to create views directly in my SQL database. This views will join all of the necessary table to return all the information that we need.
Doing so, removes a lot of boilerplate code and makes the development of the app much faster, since its easier to perform a SQL query and save the view and simply bring it in the DbContext

But the views we created dont have a primary key, let's fix that

```
  public DbSet<ChampionInfo> ChampionsInfo { get; set; }
  public DbSet<ReviewView> ReviewView { get; set; }

  modelBuilder.Entity<RegionChampionsCount>().HasNoKey(); // RegionChampionsCount is a view that does not have a key.
  modelBuilder.Entity<ReviewView>().HasNoKey();
```

Example:
```
SELECT Champions.champion_id, Champions.name, Champions.catchphrase, Champions.description, Champions.release_date, Regions.name AS region_name, Regions.emblem AS region_emblem, Roles.name AS role_name, Roles.icon AS role_icon, Images.image, icon_images.image AS champ_icons
FROM Champions
LEFT JOIN Regions ON Champions.region_id = Regions.region_id
LEFT JOIN Roles ON Champions.role_id = Roles.role_id
LEFT JOIN ChampionsImage ON Champions.champion_id = ChampionsImage.champion_id
LEFT JOIN Images ON ChampionsImage.image_id = Images.image_id
LEFT JOIN Images AS icon_images ON Champions.icon_id = icon_images.image_id
```

## Technologies used

### C# .NET Core 7, SQL Server (local).
