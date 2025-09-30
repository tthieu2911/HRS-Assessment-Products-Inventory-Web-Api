#!/bin/bash

# Define results directory (relative to project root)
RESULTS_DIR="./test-results"

# Remove old results
rm -rf "$RESULTS_DIR"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory "$RESULTS_DIR"

# Generate HTML coverage report
reportgenerator \
  -reports:"$RESULTS_DIR"/*/coverage.cobertura.xml \
  -targetdir:"$RESULTS_DIR/coveragereport" \
  -reporttypes:Html
