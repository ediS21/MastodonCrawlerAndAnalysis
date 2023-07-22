import json
import networkit as nk
import time

G_nk = nk.Graph()  # NetworKit graph
id_to_index = {}  # mapping from original id to integer index
counter = 0

# Open and read the JSON file
with open('MSocial-followingData\Following87k.json', 'r', encoding='utf-8') as file:
    for line in file:
        counter += 1
        print(counter)
        line = line.strip()
        if not line:
            continue
        json_obj = json.loads(line)
        for item in json_obj:
            source_id = item.get('source_id')
            target_id = item.get('target_id')

            if source_id is None or target_id is None:
                continue  # if either source_id or target_id is None, we skip this iteration

            source_id = str(source_id)
            target_id = str(target_id)

            # map original ids to integer indices
            if source_id not in id_to_index:
                id_to_index[source_id] = G_nk.addNode()
            if target_id not in id_to_index:
                id_to_index[target_id] = G_nk.addNode()

            # Add edge
            if source_id and target_id:
                G_nk.addEdge(id_to_index[source_id], id_to_index[target_id])

# Degree centrality
degree = nk.centrality.DegreeCentrality(G_nk,True)
degree.run()

degree_centrality = {str(node): degree.score(node) for node in G_nk.iterNodes()}
sorted_nodes_degree = sorted(degree_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_degree = sorted_nodes_degree[:20]

with open('degreeNK.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_degree, start=1):
        source_id = [k for k,v in id_to_index.items() if v == int(node)][0]
        line = f"Rank: {rank}: ID: {source_id}\tDegree Centrality: {centrality}\n"
        file.write(line)
        print(line)

# Eigenvector centrality
eigenvector = nk.centrality.EigenvectorCentrality(G_nk)
eigenvector.run()

eigenvector_centrality = {str(node): eigenvector.score(node) for node in G_nk.iterNodes()}
sorted_nodes_eigenvector = sorted(eigenvector_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_eigenvector = sorted_nodes_eigenvector[:20]

with open('eigenvector.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_eigenvector, start=1):
        source_id = [k for k,v in id_to_index.items() if v == int(node)][0]
        line = f"Rank: {rank}: ID: {source_id}\tEigenvector Centrality: {centrality}\n"
        file.write(line)
        print(line)


# Closeness centrality
closeness = nk.centrality.Closeness(G_nk, False, nk.centrality.ClosenessVariant.Generalized)
start_time = time.time()
closeness.run()
end_time = time.time()
closeness_computation_time = end_time - start_time

closeness_centrality = {str(node): closeness.score(node) for node in G_nk.iterNodes()}
sorted_nodes_closeness = sorted(closeness_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_closeness = sorted_nodes_closeness[:20]

with open('closeness.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_closeness, start=1):
        source_id = [k for k,v in id_to_index.items() if v == int(node)][0]
        line = f"Rank: {rank}: ID: {source_id}\tCloseness Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Closeness Centrality:", closeness_computation_time)

# Betweenness centrality using networkit
betweenness = nk.centrality.Betweenness(G_nk,True)
start_time = time.time()
betweenness.run()
end_time = time.time()
betweenness_computation_time = end_time - start_time

betweenness_centrality = {str(node): betweenness.score(node) for node in G_nk.iterNodes()}
sorted_nodes_betweenness = sorted(betweenness_centrality.items(), key=lambda x: x[1], reverse=True)
top_20_nodes_betweenness = sorted_nodes_betweenness[:20]

with open('betweenness.txt', 'w') as file:
    for rank, (node, centrality) in enumerate(top_20_nodes_betweenness, start=1):
        source_id = node
        line = f"Rank: {rank}: ID: {source_id}\tBetweenness Centrality: {centrality}\n"
        file.write(line)
        print(line)

print("Betweenness Centrality:", betweenness_computation_time)

