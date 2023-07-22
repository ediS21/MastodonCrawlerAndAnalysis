# Mastodon Data Crawler And Analysis

This repository contains the codebase for the data collection and analysis methodologies employed in my Bachelor's Thesis, focused on instance network analysis of Mastodon.

# Collecting Data

The data collection process uses the C# programming language, with the Mastonet library fascilitating access the Mastodon API. It's important to remember to incorporate this package into your DOTNET environment prior to executing the code.

To install the Mastonet package into your DOTNET environment, navigate to the particular C# folder you'd like to run and input the subsequent command into the terminal:

```
dotnet add package Mastonet --version 2.3.1
```

For more detailed instructions on integrating the Mastonet package into DOTNET, visit this link: [NuGet: Mastonet](https://www.nuget.org/packages/Mastonet/)

For more context or insight into the Mastonet library itself, visit its GitHub page: [GitHub: Mastonet](https://github.com/glacasa/Mastonet)

# Analyzing Data

In order to analyze the data, Python and a variety of libraries are used:

- Pandas and matplotlib are utilized for the creation of Evolution graphs.
- NetworKIT used for the centrality analysis.
- NetworkX used for community and clique analysis.

To install these libraries, navigate to the Python sub-folder located within the data analysis folder and input the following command into the terminal, replacing `` with the desired library:

```
pip install <library_name>
```

For more information on these Python libraries, check out their respective documentation:

- [Pandas](https://pandas.pydata.org/)
- [matplotlib](https://matplotlib.org/stable/index.html)
- [NetworkX](https://networkx.org/documentation/stable/index.html)
- [[NetworKIT](https://networkit.github.io/)](https://networkit.github.io/)