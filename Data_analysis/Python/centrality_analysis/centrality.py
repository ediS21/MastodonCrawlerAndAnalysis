import networkx as nx
import time
import json

#NetworkX version of Centrality - computes too slow on Habrok - Not used in the results of analysis.

# Read the data from the JSON file
data = []
with open('FollowingSubset.json', 'r') as file:
    for line in file:
        json_obj = json.loads(line)
        data.extend(json_obj)

# Create a graph and add nodes and edges
graph = nx.Graph()

counter = 0

for item in data:
    counter += 1
    source_id = item['source_id']
    target_id = item['target_id']
    
    # Add edge only if both source_id and target_id are not None
    if source_id is not None and target_id is not None:
        graph.add_edge(source_id, target_id)
    print(counter)

#Degree centrality 
start_time = time.time()
degree_centrality = nx.degree_centrality(graph)
sorted_nodes_degree = sorted(degree_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_degree = sorted_nodes_degree[:20]
end_time = time.time()
degree_computation_time = end_time - start_time

with open('degree.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_degree, start=1):
        source_id = node
        line = f"Rank: {rank}: ID: {source_id}\tDegree Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Degree Centrality:", degree_computation_time)

#Betweenness centrality
start_time = time.time()
betweenness_centrality = nx.betweenness_centrality(graph)
sorted_nodes_betweenness = sorted(betweenness_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_betweenness = sorted_nodes_betweenness[:20]
end_time = time.time()
betweenness_computation_time = end_time - start_time

with open('betweenness.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_betweenness, start=1):
        source_id = node
        line = f"Rank: {rank}: ID: {source_id}\tBetweenness Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Betweenness Centrality:", betweenness_computation_time)

#Closeness centrality
start_time = time.time()
closeness_centrality = nx.closeness_centrality(graph)
sorted_nodes_closeness = sorted(closeness_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_closeness = sorted_nodes_closeness[:20]
end_time = time.time()
closeness_computation_time = end_time - start_time

with open('closeness.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_closeness, start=1):
        source_id = node
        line = f"Rank: {rank}: ID: {source_id}\tCloseness Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Closeness Centrality:", closeness_computation_time)

#Eigenvector centrality
start_time = time.time()
eigenvector_centrality = nx.eigenvector_centrality(graph)
sorted_nodes_eigenvector = sorted(eigenvector_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_eigenvector = sorted_nodes_eigenvector[:20]
end_time = time.time()
eigenvector_computation_time = end_time - start_time

with open('eigenvector.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_eigenvector, start=1):
        source_id = node
        line = f"Rank: {rank}: ID: {source_id}\tEigenvector Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Eigenvector Centrality:", eigenvector_computation_time)