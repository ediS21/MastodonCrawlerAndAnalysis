import json
from collections import defaultdict
import networkx as nx
from collections import Counter

#Reads following data from json file -> creates undirected graph -> retrieves all components of the undirected graph ->
# -> for each component we find cliques -> output the information we want in txt file.

# Read the data from the JSON file
data = []
with open('test2.json', 'r', encoding='utf-8') as file:
    for line in file:
        line = line.strip()
        if line:
            json_obj = json.loads(line)
            data.extend(json_obj)

# Create a dictionary where each key is a source_id, and each value is a set of target_ids that the source follows.
follows_dict = defaultdict(set)
for datum in data:
    source_id = str(datum["source_id"])
    target_id = str(datum["target_id"])
    profile_url = datum["ProfileUrl"]

    # Only add target_id to the set of follows for source_id if the profile url contains "mastodon.social"
    if profile_url and "mastodon.social" in profile_url:
        follows_dict[source_id].add(target_id)

cnt = 0

# Identify pairs where both nodes follow each other
mutual_follows = set()
for source_id, target_ids in follows_dict.items():
    for target_id in target_ids:
        # Check if they are mutual follows and have not been added yet
        if source_id in follows_dict.get(target_id, set()):
            cnt += 1
            print(cnt)
            # Sort the pair to ensure each pair is added only once
            mutual_follow = tuple(sorted([source_id, target_id]))
            mutual_follows.add(mutual_follow)

# Convert each pair to a dictionary
mutual_follows_list = [{"node_id1": pair[0], "node_id2": pair[1]} for pair in mutual_follows]

# Create a graph from the pairs
G = nx.Graph()
G.add_edges_from(mutual_follows)

# Get all connected components (undirected subgraphs) of the graph
connected_components = nx.connected_components(G)

# Initialize variables
max_graph_size = 0
total_cliques = 0
largest_clique_size = 0
total_graphs = 0
large_graphs = 0
clique_count = 0
node_counts = Counter()

largest_cliques = []

# Process each component
for component in connected_components:
    total_graphs += 1
    component_graph = G.subgraph(component)
    if component_graph.number_of_nodes() > max_graph_size:
        max_graph_size = component_graph.number_of_nodes()

    if component_graph.number_of_nodes() > 2:
        large_graphs += 1
        cliques = list(nx.find_cliques(component_graph))
        total_cliques += len(cliques)
        clique_count += 1
        for clique in cliques:
            if len(clique) > largest_clique_size:
                largest_clique_size = len(clique)
                largest_cliques = [clique]
            elif len(clique) == largest_clique_size:
                largest_cliques.append(clique)
            if len(clique) > 1:
                for node in clique:
                    node_counts[node] += 1

# Calculate average number of cliques in large graphs
avg_cliques = total_cliques / clique_count if clique_count > 0 else 0

# Get top 20 nodes that are contained most frequently in more than 1 clique
top_nodes = node_counts.most_common(20)

# Write the results to an output text file
with open('results3.txt', 'w') as file:
    file.write(f"Total number of undirected graphs: {total_graphs}\n")
    file.write(f"Number of undirected graphs with more than 2 nodes: {large_graphs}\n")
    file.write(f"Average number of cliques in large graphs: {avg_cliques}\n")
    file.write(f"Size of the largest graph: {max_graph_size}\n")
    file.write(f"Total number of cliques: {total_cliques}\n")
    file.write(f"Size of the largest clique: {largest_clique_size}\n")
    file.write(f"Top 20 nodes that are contained most frequently in more than 1 clique: {top_nodes}\n")
    file.write("Largest cliques:\n")
    for clique in largest_cliques:
        file.write(f"{clique}\n")


print("Process finished")
