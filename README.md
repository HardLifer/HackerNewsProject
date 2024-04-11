# HackerNewsProject
 The solution for Hacker News test task
Hi guys

Here is my solution for the test task that I received from Danys

Solution fast, simple, and well-tested for the API

U can run it using VS, VS Code, prepared `.http` file inside of the project and ofc Docker container 

I had a few possible solutions for "efficiently service large numbers of requests without risking overloading "
1. Rate limit for calling API ( our WebAPI Client )
2. Response Cache for all requests
3. In-memory or distributed cache

I've preferred to use a simple in-memory cache as a maintainable, efficient solution coz we can replace it with a Distributed cache (Redis i.e)

However, the HackerNews API is straightforward and returns always a bunch of data it is not simple to optimize it per smth
Pagination is not suit here coz we need to return N data for the user ( yes we can still use it, but we always need to check the cache per call - slow and so gallantly)

