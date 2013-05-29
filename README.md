ubjson-sharp
============

UBJSON Serializing and Deserializing Library in C#

"Draft9" implements the official draft 9 spec of UBJSON.

"Draft10" implements a hypothetical version of draft 10 spes for UBJSON that I am proposing/considering.

In the near future I will be:
- adding a number of unit tests for draft 9 to add as much coverage as I can for potential edge cases
- adding serialization of STC array and object types (as appropriate for input types) in draft 10
- adding extensive unit test coverage for draft 10 to cover the STC array and object types
- exploring a refactoring of the serializing and deserializing classes to perhaps structure things a bit more object-y -- we'll see
- adding stream listening with invoking a supplied handler for new objects as they are received over the wire
- eventually lining draft 10 up with whatever the finalized spec becomes
