File Organization
=================

Introduction
------------

This document describes the file organization conventions used in the .NET
driver. The goal is to have a standard that it is simple and conforms to
commonly used conventions.

Starting point
--------------

Our base starting point is:

Framework Design Guidelines (2nd Edition)
By Krzysztof Cwalina and Brad Abrams

Which we use almost verbatim with a few minor modifications (see below).

Framework Design Guidelines has the following to say:

- Do not have more than one public type in a source file, unless they differ
only in the number of generic parameters or one is nested in the other
- Do name the source file with the name of the public type it contains
- Do organize the directory hierarchy just like the namespace hierarchy
- Consider grouping members into the following sections in the specified
order:
-- All fields
-- All constructors
-- Public and protected properties
-- Methods
-- Events
-- All explicit interface implementations
-- Internal members
-- Private members
-- All nested types
-- Do use #region blocks around not publicly callable and explicit
interface implementation groups.
- Consider organizing members of each group in alphabetical order
- Consider organizing overloads from the simplest to the most complex
- Do place using directives outside the namespace declaration

Similar guidelines are also available at this blog post by Brad Abrams:

http://blogs.msdn.com/b/brada/archive/2005/01/26/361363.aspx

Since this is a well documented and presumably widely used standard we
are using it as our starting point.

Modifications and clarifications
--------------------------------

The conventions described in .NET Framework Design Guidelines are missing
some member types, and are silent about some issues.

We use the following slightly modified guidelines:

- Group the members into the following sections in the specified order:
-- All fields
-- All constructors
-- All events
-- All properties
-- All operators (including explicit and implicit conversions)
-- All methods
-- All explicit interface implementations
-- All nested types

Within each section, order members as follows:
- static before instance
- in the case of fields, readonly before mutable
- then by visibility: public, protected, internal, private
- then by alphabetical order
- order overloads by parameter alphabetical order (in case of
doubt use Visual Studio's drop down of member names and use the same 
order Visual Studio displays them in)

Exception: constructors are sorted only alphabetically (by parameter types and names), not by visibility.

- Precede each section with a comment, e.g.:
    // private static fields
	// private fields
	// static constructor
	// constructors
	// public static properties
	// public properties
	// etc...

Miscellaneous
-------------

Extension methods should be defined in a static class named XyzExtensionMethods.cs.
