# **Project structure**

The solution is split into three layers, top to bottom they are: 

- ### API

- ### Business Logic

- ### Data

  

## **API**

The API layer is primarily where HTTP requests and responses are handled. Aside from offloading the actual processing of the requests to the business logic layer, and aspects such as authorisation and rate limiting, that's pretty much all the API layer should be doing. Keep it thin, responsive and scalable.

This layer centres around the Controllers, which whilst remaining thin still do make up the bulk of the API layer. They should all be asynchronous, there's no reason not to be, almost all calls in a CRUD system touch the database (or at least a cache of it) and is this is likely to be a bottleneck in the system it's essential that they operate asynchronously to reduce this as much as possible. 

Whilst it's often the case that a controller will use a single correspondingly named service (and repo) that's not a rule, controllers can use multiple services, and multiple controllers can all use the same service (like an audit, or logging service for example).



## **Business Logic**

If the API is the doorman of the system, dealing with the input/output, security, rates, etc and the Data layer is the stock room, dealing with the querying, storing and retrieving of data, then the Business Logic layer is everything in between; the shop floor if you like. This is where the actual logic of the application lives.

Setting aside technical details like APIs, HTTP and databases, the business layer models the domain, its actions and interactions. DTOs are used to create partial views of entities, not only to protect potentially sensitive data from being leaked, but also to remove irrelevant fields and reduce the amount of data on the wire. Services represent functionality available to the domain.



## **Data**

The Data layer abstracts away the technicalities of querying a data store, and presents an interface that accepts and returns objects. There's a generic base class for a repository that does the basic CRUD operations. 

In almost any application with a data store, the data layer is usually the bottleneck of the system, so trying to keep query results as fast and lightweight as possible is paramount. Repositories in the Data layer therefore return results as IQueryables. I've written on my [blog](https://blog.davetoland.com) about this, but in short by returning an IQueryable rather than say an IEnumerable, we're deferring the execution of the query. Crucially this means two things, queries can then be further tuned by the caller via projections, filtering, ordering, paging etc to suit the individual use case, and queries can be broader and fewer in number keeping the repositories more concise. By not requiring fine grained queries for every slightly different column selection, sort order, page size, etc to be defined.. we cut down on the weight and complexity of the code. By offloading the fine tuning of the query to the business layer, whilst retaining the structure and logic of the query in the data layer we can maintain the separation of concerns whilst still promoting a lightweight and scalable architecture.

Having a fast API generally comes down to how fast you can tune your data layer, and thin minimal result sets are a good way to achieve it. We could also employ something like GraphQL here to further improve this, especially relevant if the API client is a mobile device over a cellular connection.



# ***Repository* and *Unit Of Work* Patterns**

I hesitated about using these, especially in what is a trivial use case. Entity Framework itself already implements these patterns, a context is a unit of work, a DbSet is a repository, and by implementing these patterns again it is sometimes argued that you're just adding further layers of indirection on top for no reason. 

But I disagree. For a start that means exposing Entity Framework and the DbContext to the business layer, and concepts like keys and DbSets are not part of the business domain. Secondly, this often ends up with several different implementations of say an Update method, or a way to add a new entity. By implementing a customised repository pattern on top of Entity Framework, you're providing an interface to the business layer that's clear and unambiguous, and suited more to the domain than a database. Rather than expecting your domain objects to makes calls like: `DbContext.Notes.Where(n => n.TicketId == ticketId)` they'd be more suited making calls like: `NotesRepo.GetByTicketId(ticketId)`.

The Repository pattern enables the creation of a platform that is both easy to scale, and easy to make global customisations. The base repository can be used as is, if an entity has no functionality to add, or it can be extended and customised on a per entity basis. This helps keep things as lean and DRY as possible.

There are two main benefits to using the Unit Of Work pattern. Firstly it encapsulates the concept of transactions, enabling atomic changes on repositories through a Commit method, without exposing the underlying Entity Framework to the business layer. It also makes managing repositories much easier for services, producing a concise constructor signature, rather than needing to pass individual repositories as parameters. Only a single assignment and field need to be made. It doesn't sound much, but when the system contains multiple services that pull in multiple repos each, things can get messy, especially if and when change needs to happen.  

Instead of using these patterns we could just expose a DbContext to the business layer, that would enable calls to the SaveChanges method and maintains the aspect of transactional commits, but that exposes EF to the Business layer and forces it to write data queries in the domain logic. You could have SaveChanges calls hard coded into repository methods, so that every create/update/delete operation is saved immediately, but then that sacrifices the benefits of having operations batched into atomic transactions. These patterns do incur some initial effort to setup, however once implemented this is offset by the separation, encapsulation and ease-of-use they provide.



# **DbEntity vs DTO vs API Model**

Another thing I hesitated over. Do we really need three classes to describe each piece of data? The golden question. With a use case like this, probably not, but then we're probably not building systems that only contain 3 or 4 entities. When I've interviewed candidates for roles like this before I've made the test fairly brief but I'm looking to see how candidates go about designing the API with scalability in mind, for me that's quite high on the agenda with an enterprise solution. So I've done it this way...

The DbEntity represents a row in the database, the DTO represents an entity in the business domain (there can be multiple DTOs representing different subsets or aspects of a DbEntity), and the API Model is a container for incoming data with built in validation that's triggered during automatic model binding.

When you're dealing with a very large database, especially where you have sensitive data in your store (which most do), and when you want fine grained control over the shape of incoming data, then this is a model that will work and scale with the application as it matures. The DbEntity (or at least classes in the DbContext) are of course required, generally you don't want to pass these out of the API as they contain sensitive data so you create DTOs to provide views of them. Some implementations stop here, and use the DTOs as containers for the incoming data, or even worse have method signatures that contain a billion input params, but this either results in input validation and JsonIgnore attributes being mixed in with output data which can quickly get unmanageable - or, you have no model validation and take care of that with a ton of hand cranked if statements. In my experience it doesn't take long for a system to grow to the level when this *tripled up* approach actually reduces the amount of code you write.



# **Authentication/Authorisation**

Normally I wouldn't consider adding auth to a demo project, it's generally out of scope. However, the spec asked for the Delete method on a Note to be restricted to Admins only, and the way I'd normally do this is through ASP.NETs built in libraries using the Authorize attribute. So that's what I've done. It was easier to lean on knowledge I already had than try to come up with a hybrid prototype way of doing it. I've kept the login process as lightweight as possible, requiring only the surname of an existing Person to get a JWT token. Obviously in a real scenario you'd require a unique identifier and a password here, or a cert, 2FA, etc.

This also means the Auth controller just does a simple JWT token creation inline, but ordinarily you'd include a service here, and perform business actions like registration, lost password, checking the user is not currently banned, or temporarily locked out, any auditing and logging, etc in that. All that's been left out for brevity. 

To authenticate via Swagger, just execute the Login method and copy the resulting token from the response. Click the *Authorize* button, and then paste the token into the text box. To switch to a different user, repeat the process with a different user.

Incidentally, auth has been added to the Ticket and Note controllers, but not Person. Solely because without a proper onboarding process, you'll never be able to use the system as you won't even be able to create a user to login with. To that end, it was just easier to leave the Authorize off of Person, but of course in a real world scenario you'd have a full registration process and login capabilities (duplicate user check, forgotten password facility, checks that user is not banned, from a forbidden location, etc) to handle this.



# **Testing**

Unit Tests have been written with NUnit, using Moq, AutoFixture and Fluent Assertions. Due to the *base* controller, service and repository classes, these tests are able to remain fairly DRY. Once the tests for these base classes have been written, it minimises repetition when adding new controllers/services/repos.

I would usually include ExcludeFromCodeCoverage attributes wherever needed, I'm a fan of marking irrelevant things with this and then aiming for 100% code coverage, which can be automatically triggered (and gated on with say SonarQube) as part of the CI process.

Integration tests could be added using Specflow or Selenium, all of which can be also be triggered and gated on during CI. 