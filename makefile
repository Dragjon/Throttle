# Makefile for Chess-Challenge project

# Compiler
CSC = dotnet

# Solution directory
SOLUTION_DIR = C:\Users\dragon\Documents\Throttle\Chess-Challenge.Uci

# Main project directory
MAIN_PROJECT_DIR = $(SOLUTION_DIR)\Chess_Challenge.Cli

# Build target
build:
    $(CSC) build $(MAIN_PROJECT_DIR)\Chess-Challenge.Cli.csproj

# Clean target
clean:
    $(CSC) clean $(MAIN_PROJECT_DIR)\Chess-Challenge.Cli.csproj
