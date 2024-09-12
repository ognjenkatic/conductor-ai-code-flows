This project is a [hackathon entry](https://buildtheflow.devfolio.co/).
This project utilizes a neat little [open source conductor client library](https://github.com/codaxy/conductor-sharp) for polling tasks, registering workers, creating workflow definitions.

Description copied from hackathon entry:

# Project Overview

It provides a flexible way of performing automated code refactoring. For the Hackathon entry, the project provides refactoring capabilities for .NET alone, however, the principles of Conductor make expanding this support much easier.

 The usage of Conductor here allows:
- Detailed process overview by looking through tasks, workflows, logs to see the progress of refactoring.
- Easier collaboration between multiple teams potentially using different technologies. The project presents multiple microservices, focused on AI, .NET specific matters, git repo management matters. For real world application these microservices would be expanded with much more functionality, and additional ones would be added to cover other languages, such as python, by teams who are proficient at its usage. The integration of these new functionalities would remain equally simple thanks to the orchestration provided by Conductor. Additionally, tasks that deal with things like refactoring individual files with AI can be reused (providing some changes to the task to tune the prompt).
- Handling rainy day scenarios that affect multiple different microservices using failure workflows.
- Not having to worry about retries (AI coding can be unpredictable), rate limits (AI coding can be slow when many workflows are run), and much more.
- Dynamic execution of appropriate subworkflow. The project contains two main workflows, one that deals with cloning the repository and git related matters, and one that performs refactoring of the project. The second workflow is dynamically determined and started using naming conventions. Support for additional programming language refactoring can be achieved by creating new workflows fitting the naming convention.

- ## Limitations
This project is in an MVP phase, this means things might break, they might not always work, there are missing optimizations, etc. It is supposed to demonstrate the capability and allow for building upon the idea of AI refactoring that is integrated into the standard development flow. Additionally there are some limitations:
- Only my own projects are whitelisted for refactoring as I would not wish to spam other peoples repositories with unwanted PRs
- Only one PR per repository is allowed, once PRs have been created for all the whitelisted repositories, the workflow will fail for each new one. I will try to delete these to allow for multiple runs, but will not really invest much effort into this, as the code is open source and you can poke around to see how it works. The portal is there purely to demonstrate it actually runs :)
- There is a rate limit here as the website requires no auth, if you keep encountering it and cannot get the refactor to run, get in touch on GitHub and i can help you out.
- Shallow integration with metrics tools that are used. They actually provide quite a lot of functionality that would be beneficial and could be used to make the AI code generation result in better code. However, due to the duration of the hackathon a simpler approach was taken.
-AI sometimes makes changes that break related classes/files. This was a tough one, it kept resisting prompts that forbit id to change the names of public classes and so on. In the end a combination of prompts and actual filtering in code was done. However, this was not 100% effective and it still sometimes makes these changes, this is something that should be resolved in the production ready version.

![refactor-csharp](https://github.com/user-attachments/assets/9082cf61-d3e5-4a74-a21c-d167abb3f27b)
![refactor-repo-p1](https://github.com/user-attachments/assets/eb354d7e-6bca-42be-b673-c37a6c2acf99)
![refactor-repo-p2](https://github.com/user-attachments/assets/f4bd9420-7487-4601-b297-dad5663cbe55)
![refactor-run](https://github.com/user-attachments/assets/9c14a2cd-5e70-41ab-8c0f-6e7aa39bce93)
![refactor-runs](https://github.com/user-attachments/assets/3984e8a5-1bec-4973-8159-5e3d4573771c)
