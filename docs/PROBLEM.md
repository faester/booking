# Problem:
Your company wants to arrange a list of conventions as part of supporting the strong fan culture.
You have identified a need for a new system to manage the convention bookings, events, talks and venues.

# Complexities:
The solution should support the ability for participants to register for conventions, as well as reserve seats in
any interesting talks. Convention administrators need to create and maintain the events and venues, and
talkers should be able to register a talk for a specific convention.

A typical participant registration could include the following information: name, address, phone number and
email.

# Your tasks:
Develop and present a solution that facilitates conventions with a series of talks.
Focus on the following aspects when creating the solution:
- Authentication &amp; Authorization using OpenID Connect
- Security implications
- Privacy & legal constraints
- High performance and the ability to handle high concurrent users in a short time

## Inspirational input:
- We are using OpenID Connect and can recommend this source or other SaaS identity providers 
   -  https://auth0.com provides a free OpenID Connect service  and a number of example projects https://auth0.com/docs/quickstarts
- If breweries could work as venues you could use this API to select them
   -  https://www.openbrewerydb.org/#documentation
- If Marvel were to be the theme of the conventions you could add topics from this API   
   -  https://developer.marvel.com/documentation/getting_started

Any other API you know that could be interesting to use

We do not expect a complete solution to the presented problem. We recommend to stub parts of the solution
to get into depth on other parts. It is up to you to select the focus areas that best covers your skillset and
your area of interest.

# Constraints for the case:
- Solution must be written in C#
- Solution must use OpenID Connect
- For any frontend code we recommend React or similar framework, but frontend is not the focus area
of this case
- Solution must be designed to run on Amazon Web Services or Azure
- You should use no more time than you find reasonable, we do not expect a fully complete solution. We are more interested in the choices you make along the way
- Presentation must take no more than 25 minutes - including time for questions
# Expected outcome:
- Short presentation of the solution
- Explain what OpenID Connect is to product owner
- Explain what OpenID Connect is to a developer
- Explain what certified library you know the best and you would recommend?
- Context and Container level diagrams of the solution in the C4 format (https://c4model.com)
- Explanation of grant types (flow). Which should be used when and when not?
- Explanation of choice of user flow for web
- Explanation of choice of user flow if the solution is used from a mobile app
- Explain how a user flow works
- Explanation of what PKCE, Nonce, State and Redirect URI do and who “owns” them
- Explain the Duties and roles of clients, resources, the authority, the browser
- Explain how trust is established between a web server and the IDP?
- Explain how custom scopes could be used in this app? What benefits do they provide? What
disadvantages? What do they protect against? How are scopes connected to JWT?
- A developer is implementing a solution and wants to know whether a user is logged in. How can that
be achieved? If done in a browser client-side, how is the communication protected in the browser?
- GitHub project with all files relevant for the case

# What are we testing:
- Technical, Communication &amp; Presentation skills
- Domain knowledge of system design
- Domain knowledge of OpenID Connect
- Code &amp; Knowledge of backend web development
- Ability to understand and use APIs
- Creating a solution with limited input and within a limited frame
- Learning mindset - how you gather the information needed to move on
