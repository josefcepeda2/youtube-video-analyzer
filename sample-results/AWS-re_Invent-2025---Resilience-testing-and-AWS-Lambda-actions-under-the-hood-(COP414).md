Of course. Here is a detailed analysis and summary of the YouTube video transcript from the presentation on Chaos Engineering for AWS Lambda.

### **Video Information**

*   **Title:** How to get started with chaos engineering for AWS Lambda (SVS310)
*   **URL:** [https://www.youtube.com/watch?v=AnegpGnS-cY](https://www.youtube.com/watch?v=AnegpGnS-cY)
*   **Speakers:** Saurabh Kumar (Senior Solutions Architect, AWS) and Haresh Nandwani (Solutions Architect, AWS)

---

### **Executive Summary**

This presentation is a technical deep-dive into performing chaos engineering on AWS Lambda functions using newly introduced native fault actions within the AWS Fault Injection Service (FIS). The speakers, Saurabh Kumar and Haresh Nandwani, establish that while Lambda is inherently resilient, applications built with it are part of a larger system whose overall resilience must be tested. They introduce three specific FIS actions for Lambda (`add_start_delay`, `modify_integration_response`, `invocation_errors`) and explain the underlying mechanism: a managed Lambda Extension that polls an S3 bucket for fault configurations. The core of the session is a comprehensive live coding demonstration using the AWS Cloud Development Kit (CDK) to deploy a sample serverless application, configure it for FIS, define and deploy FIS experiment templates, and analyze the results using CloudWatch dashboards. Key themes include the importance of observability, the safety mechanisms within FIS, and the practical application of resilience testing as code.

---

### **Detailed Session Breakdown**

#### **1. Introduction and Problem Statement**

*   **Audience Context:** The speakers begin by polling the audience, confirming that a majority use Lambda in production but a much smaller number perform resilience testing on it. This frames the session's relevance.
*   **The Serverless Resilience Challenge:** While serverless platforms like Lambda remove the burden of infrastructure management, they can also make it more difficult to perform traditional resilience testing or chaos engineering because the underlying infrastructure is abstracted away.
*   **The Solution:** AWS FIS native fault actions are introduced as the solution to this problem, providing an easy and quick way to get started with chaos engineering for serverless workloads.

#### **2. Overview of Chaos Engineering and AWS Fault Injection Service (FIS)**

*   **What is Chaos Engineering?** It's defined as the practice of experimenting on a system to build confidence in its capability to withstand turbulent conditions in production. It involves proactively injecting faults to validate how the system responds. The primary benefit is discovering "unknown unknowns" before they cause production outages.
*   **Why use AWS FIS?** Three key reasons are provided:
    1.  **Fully Managed and Serverless:** No infrastructure to manage for the chaos tooling itself. It includes a "failure scenario library" to help users get started quickly.
    2.  **Native AWS Integration:** FIS can perform actions directly on AWS services (e.g., pausing an Auto Scaling group), which is a unique advantage over many third-party tools.
    3.  **Controls and Guardrails:** It's designed for safe experimentation. Features like "safety levers" allow for the immediate termination of all experiments if something goes wrong, ensuring a controlled "blast radius."
*   **Key FIS Terminology:**
    *   **Experiment Template:** A blueprint for a chaos experiment, defining the actions, targets, and stop conditions.
    *   **Experiment:** An instance of running an Experiment Template.
    *   **Actions:** The faults to be injected (e.g., "shutdown EC2 instance," "add latency").
    *   **Targets:** The AWS resources on which the actions are performed (e.g., a specific Lambda function or a set of instances with a certain tag).
    *   **Observability:** The critical role of Amazon CloudWatch is emphasized. The mantra is, "If you can't observe it, it hasn't happened." You must be able to measure the impact of an experiment.

#### **3. Lambda Resilience and New FIS Actions**

*   **Inherent Resilience:** Lambda is presented as a resilient service by design, leveraging multiple Availability Zones (AZs) within a region to handle infrastructure failures with minimal impact on the user's function.
*   **The Need for Application-Level Testing:** The speakers clarify that even with a resilient service like Lambda, the entire application's resilience is not guaranteed. Lambda functions rarely operate in isolation; they integrate with upstream and downstream systems. Therefore, it's crucial to test how the overall system behaves when the Lambda function itself experiences issues.
*   **Three New FIS Actions for Lambda:**
    1.  **Add Start Delay:** Injects a specified delay before the Lambda function handler is invoked. This is useful for testing how downstream consumers handle timeouts and increased latency.
    2.  **Modify Integration Response:** Forces the Lambda function to return an incorrect or custom response, simulating errors from downstream services that the Lambda might integrate with.
    3.  **Invocation Errors:** Causes the Lambda invocation to be marked as failed. This tests the retry logic and failure handling of the calling service (e.g., SQS, API Gateway).

#### **4. The Underlying Mechanism: How FIS Lambda Actions Work**

*   **Lambda Extensions:** The core technology enabling these actions is **Lambda Extensions**. The FIS actions are implemented via a managed AWS FIS extension that is added to the Lambda function as a layer.
*   **API Proxy Pattern:** The extension runs as a separate process alongside the function code. It implements an API proxy pattern, intercepting the request/response lifecycle between the Lambda runtime and the execution environment. This interception point allows it to inject the specified faults.
*   **Decoupled Architecture with S3:**
    *   When an FIS experiment is started, FIS writes a fault configuration file to a pre-configured S3 bucket.
    *   The FIS extension in the Lambda function periodically **polls** this S3 bucket.
    *   When it detects an active fault configuration, it downloads it and begins applying the specified fault (e.g., delay, error) to subsequent invocations.
*   **Polling Mechanism (A Key Nuance):**
    *   **Slow Pole Mode:** When no experiment is active, the extension polls infrequently (e.g., every 60 seconds) to minimize performance overhead. This is the default state.
    *   **Hot Pole Mode:** Once an active experiment is detected, the extension switches to a more frequent polling interval (e.g., every 20 seconds). This ensures that when the experiment concludes, the function quickly recovers to its normal state by detecting the removal of the configuration file.
    *   **Trade-off:** This dual-mode polling represents a trade-off between performance overhead (during normal operation) and rapid recovery (after an experiment ends).

#### **5. Live Coding Demonstration with AWS CDK**

This was the main segment of the presentation, demonstrating the end-to-end process.

1.  **Application Architecture:** A simple serverless API was used: an external consumer calls an API Gateway, which triggers various Lambda functions performing CRUD operations on a DynamoDB table.
2.  **CDK Stack 1: The Application:**
    *   Haresh walked through the CDK code (in Java) to deploy the application.
    *   **Key Configuration:** He highlighted the environment variables required on the Lambda function to enable the FIS extension, such as `AWS_FIS_CONFIG_LOCATION` (pointing to the S3 bucket) and `LAMBDA_EXEC_WRAPPER` (pointing to a bootstrap script).
    *   **Targeting with Tags:** Showed how tags are applied to the Lambda functions in the CDK code, which will be used by FIS to select the targets for the experiment.
    *   **Adding the Extension Layer:** The CDK code added the managed FIS extension layer to the Lambda function using its specific ARN.
3.  **CDK Stack 2: The FIS Experiments:**
    *   Saurabh then wrote the CDK code to define the FIS experiment templates.
    *   **IAM Roles and Policies:** He created the necessary IAM role and policies to grant FIS permission to read/write to the S3 buckets, list and describe Lambda functions, and read metrics from CloudWatch.
    *   **Defining the Experiment Template:** He defined an `ExperimentTemplate` resource in CDK. This included:
        *   A clear `description`.
        *   The `action` to perform (e.g., `aws:lambda:add-delay`).
        *   `actionParameters` (e.g., `delay` of 2 seconds, `duration` of 5 minutes, `invocationPercent` of 100%).
        *   The `target` configuration, selecting Lambda functions based on the previously defined `tag`.
        *   The `reportConfiguration`, pointing to a destination S3 bucket and the ARN of the CloudWatch dashboard for inclusion in the final report.
4.  **Execution and Analysis:**
    *   After deploying the CDK stacks, they navigated to the AWS Console.
    *   They showed the deployed FIS experiment template and used the "preview resources" feature to confirm that the correct Lambda functions would be targeted—a crucial safety check.
    *   They started the experiment.
    *   They showed a sample report generated by a previous run, which embeds snapshots of the CloudWatch dashboard for the pre-experiment (steady state), experiment, and post-experiment (recovery) phases. This visually demonstrated the impact of the injected fault (e.g., a spike in errors or latency) and the system's return to normal.

#### **6. New Scenarios, Resources, and Q&A**

*   **New "Grey Failure" Scenarios:** The presentation briefly mentioned two new, more advanced FIS scenarios for testing partial disruptions (not specific to Lambda):
    *   **AZ Application Slowdown:** Adds latency between resources within a single AZ.
    *   **Cross-AZ Traffic Slowdown:** Introduces packet loss for traffic between resources in different AZs.
*   **Resources:** They provided QR codes for a public GitHub repo with the demo code, the FIS workshop, and other relevant resilience documentation.
*   **Key Q&A Points:**
    *   **Why S3?** The use of S3 provides a decoupled mechanism. The Lambda function and its extension don't need to know about FIS directly; they just need to know where to look for a configuration file. This allows the system to function independently when no experiment is running.
    *   **Extension Version Management:** An astute audience member asked about the maintenance burden of hardcoding a specific version number in the Lambda layer ARN. The speakers acknowledged this is a valid operational concern and a good piece of feedback for the service team, as it requires manual updates to get the latest extension version.