# Ticketing System

**Auth**

Added to the Ticket and Note controllers, but not Person. Chiefly because without a proper onboarding process, you'll never be able to use the system as you won't even be able to create a user to login with. To that end, it was just easier to leave auth off of Person, but of course in a real world scenario you'd have a full registration and login capabilities (duplicate user check, forgotten password facility, checks that user is not banned, from a forbidden location, etc) to handle this.