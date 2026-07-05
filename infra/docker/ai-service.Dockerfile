# syntax=docker/dockerfile:1.7

# Multi-stage build for the AI service.
FROM python:3.12-slim AS build
WORKDIR /src
COPY ai-service/ ./
RUN pip install --no-cache-dir hatchling && pip wheel --no-cache-dir --wheel-dir=/wheels .

FROM python:3.12-slim AS runtime
WORKDIR /app
COPY --from=build /wheels /wheels
RUN pip install --no-cache-dir /wheels/*.whl && rm -rf /wheels
COPY ai-service/src ./src
ENV PYTHONUNBUFFERED=1
USER app
EXPOSE 8000
ENTRYPOINT ["uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "8000"]
