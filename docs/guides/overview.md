# API Overview

The Echo project consists of two main parts: 
- The core libraries that is shared amongst all platforms, and
- The various back-end libraries that implement the interfaces to describe various platforms.

In the following we will describe them briefly:

## Core

The core libraries expose the base interfaces that all platforms should implement, as well as the analysis algorithms that work on these models.

## Platforms

Different platforms have different features and instruction sets.
Therefore, for Echo to do analysis on code targeting such a platform, it needs to know certain properties about what the code looks like, and how a program evolves over time.