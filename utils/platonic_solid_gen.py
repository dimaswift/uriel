import os  
import math  

def generate_platonic_solids_cginc():  
    # Define the solids  
    SEQUENCE_COUNT = 6

    tetrahedron = [  
        "float3(0.35355339, 0.35355339, 0.35355339)",  
        "float3(0.35355339, -0.35355339, -0.35355339)",  
        "float3(-0.35355339, 0.35355339, -0.35355339)",  
        "float3(-0.35355339, -0.35355339, 0.35355339)"  
    ]  
    
    octahedron = [  
        "float3(1.0f, 0.0f, 0.0f)",  
        "float3(-1.0f, 0.0f, 0.0f)",  
        "float3(0.0f, 1.0f, 0.0f)",  
        "float3(0.0f, -1.0f, 0.0f)",  
        "float3(0.0f, 0.0f, 1.0f)",  
        "float3(0.0f, 0.0f, -1.0f)"  
    ]  
    
    cube = [  
        "float3(-0.5f, -0.5f, -0.5f)",  
        "float3(0.5f, -0.5f, -0.5f)",  
        "float3(-0.5f, 0.5f, -0.5f)",  
        "float3(0.5f, 0.5f, -0.5f)",  
        "float3(-0.5f, -0.5f, 0.5f)",  
        "float3(0.5f, -0.5f, 0.5f)",  
        "float3(-0.5f, 0.5f, 0.5f)",  
        "float3(0.5f, 0.5f, 0.5f)"  
    ]  
    
    icosahedron = [  
        "float3(0.000000, 0.525731, 0.850651)",  
        "float3(0.000000, -0.525731, 0.850651)",  
        "float3(0.000000, 0.525731, -0.850651)",  
        "float3(0.000000, -0.525731, -0.850651)",  
        "float3(0.525731, 0.850651, 0.000000)",  
        "float3(-0.525731, 0.850651, 0.000000)",  
        "float3(0.525731, -0.850651, 0.000000)",  
        "float3(-0.525731, -0.850651, 0.000000)",  
        "float3(0.850651, 0.000000, 0.525731)",  
        "float3(0.850651, 0.000000, -0.525731)",  
        "float3(-0.850651, 0.000000, 0.525731)",  
        "float3(-0.850651, 0.000000, -0.525731)"  
    ]  
    
    dodecahedron = [  
        "float3(0.577350, 0.577350, 0.577350)",  
        "float3(0.577350, 0.577350, -0.577350)",  
        "float3(0.577350, -0.577350, 0.577350)",  
        "float3(0.577350, -0.577350, -0.577350)",  
        "float3(-0.577350, 0.577350, 0.577350)",  
        "float3(-0.577350, 0.577350, -0.577350)",  
        "float3(-0.577350, -0.577350, 0.577350)",  
        "float3(-0.577350, -0.577350, -0.577350)",  
        "float3(0.000000, 0.356822, 0.934172)",  
        "float3(0.000000, -0.356822, 0.934172)",  
        "float3(0.000000, 0.356822, -0.934172)",  
        "float3(0.000000, -0.356822, -0.934172)",  
        "float3(0.934172, 0.000000, 0.356822)",  
        "float3(-0.934172, 0.000000, 0.356822)",  
        "float3(0.934172, 0.000000, -0.356822)",  
        "float3(-0.934172, 0.000000, -0.356822)",  
        "float3(0.356822, 0.934172, 0.000000)",  
        "float3(-0.356822, 0.934172, 0.000000)",  
        "float3(0.356822, -0.934172, 0.000000)",  
        "float3(-0.356822, -0.934172, 0.000000)"  
    ]  
    
    # Define sizes and names  
    solids = [  
        {"name": "TETRAHEDRON", "vertices": tetrahedron, "size": 4},  
        {"name": "OCTAHEDRON", "vertices": octahedron, "size": 6},  
        {"name": "CUBE", "vertices": cube, "size": 8},  
        {"name": "ICOSAHEDRON", "vertices": icosahedron, "size": 12},  
        {"name": "DODECAHEDRON", "vertices": dodecahedron, "size": 20}  
    ]  
    
    # Calculate total vertices  
    total_vertices = sum(solid["size"] for solid in solids)  
    
    # 120 is the LCM of all platonic solid vertex counts: 4, 6, 8, 12, 20  
    SEQUENCE_BUFFER_SIZE = 120  
    
    # Function to create ping-pong pattern from a sequence to fill exactly 120 slots  
    def create_perfect_ping_pong(sequence, target_size=SEQUENCE_BUFFER_SIZE):  
        size = len(sequence)  
        result = []  
        
        # Calculate how many complete ping-pongs we need  
        # For a sequence of length n, a complete ping-pong is 2n-2 steps  
        # (We go n steps forward, then n-2 steps back, skipping the first and last elements)  
        ping_pong_length = 2 * size - 2  
        
        # If the sequence is just 1 or 2 elements, ping-pong doesn't work the same way  
        if size <= 2:  
            cycles_needed = target_size // size  
            return (sequence * cycles_needed)[:target_size]  
        
        # Calculate how many complete cycles we need  
        cycles_needed = target_size // ping_pong_length  
        
        # Fill with complete ping-pong cycles  
        for _ in range(cycles_needed):  
            # Forward  
            result.extend(sequence)  
            # Backward (skip first and last elements to avoid duplicates)  
            result.extend(sequence[-2:0:-1])  
        
        # Handle remaining elements for perfect 120 size  
        remaining = target_size - len(result)  
        if remaining > 0:  
            # Add partial cycle if needed  
            if remaining <= size:  
                result.extend(sequence[:remaining])  
            else:  
                result.extend(sequence)  
                result.extend(sequence[-2:-(remaining-size+2):-1])  
        
        return result[:target_size]  
    
    # Generate sequence patterns  
    def generate_sequence_patterns(size):  
        patterns = []  
        
        # Original sequence  
        original = list(range(size))  
        patterns.append(original)  
        
        # Reversed sequence  
        reversed_seq = list(reversed(original))  
        patterns.append(reversed_seq)  
        
        # Alternating front/back sequence  
        alternating = []  
        for i in range((size + 1) // 2):  
            alternating.append(i)  
            if i != size - 1 - i:  # Avoid duplicating middle element  
                alternating.append(size - 1 - i)  
        patterns.append(alternating)  
        
        # Split and reverse halves  
        midpoint = size // 2  
        first_half = original[:midpoint]  
        second_half = original[midpoint:]  
        patterns.append(first_half + second_half[::-1])  
        patterns.append(first_half[::-1] + second_half)  
        
        # Mid-out pattern  
        mid_out = []  
        mid = size // 2  
        for i in range(size):  
            if i % 2 == 0:  
                mid_out.append((mid + i//2) % size)  
            else:  
                mid_out.append((mid - (i+1)//2 + size) % size)  
        patterns.append(mid_out)  
        
        # Fibonacci-like pattern (each element is sum of previous two indices, mod size)  
        fibonacci = [0, 1]  
        for i in range(2, size):  
            next_val = (fibonacci[i-1] + fibonacci[i-2]) % size  
            fibonacci.append(next_val)  
        patterns.append(fibonacci)  
        
        # Prime number steps  
        primes = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37]  
        for prime in primes[:3]:  # Use a few prime patterns  
            prime_pattern = [(i * prime) % size for i in range(size)]  
            if prime_pattern not in patterns:  
                patterns.append(prime_pattern)  
        
        # Generate more patterns by combining existing ones  
        while len(patterns) < SEQUENCE_COUNT:  
            # Take an existing pattern and apply transformations  
            pattern_index = len(patterns) % 8  # Cycle through the 8 base patterns  
            base_pattern = patterns[pattern_index].copy()  
            
            # Apply offset (shift pattern)  
            offset = len(patterns) % size  
            shifted = [(i + offset) % size for i in base_pattern]  
            
            # Apply step (take every nth element)  
            step = (len(patterns) % 3) + 1  
            if step > 1:  
                stepped = [base_pattern[i] for i in range(0, size, step)]  
                stepped += [base_pattern[i] for i in range(1, size, step)]  # Add remaining elements  
                shifted = stepped[:size]  # Ensure we have exactly 'size' elements  
            
            patterns.append(shifted)  
            
            # Break if we've generated enough patterns  
            if len(patterns) >= SEQUENCE_COUNT:  
                break  
        
        return patterns[:SEQUENCE_COUNT]  
    
    # Generate the CGINC file content  
    cginc_content = "#ifndef PLATONIC_SOLIDS_INCLUDED\n"  
    cginc_content += "#define PLATONIC_SOLIDS_INCLUDED\n\n"  
    
    # Define the platonic solids enum  
    cginc_content += "// Platonic Solids Enum\n"  
    cginc_content += "enum Solid\n"  
    cginc_content += "{\n"  
    for i, solid in enumerate(solids):  
        cginc_content += f"    {solid['name']} = {i},\n"  
    cginc_content += "};\n\n"  
    
    # Define the unified sequence struct  
    cginc_content += "// Unified sequence struct for all platonic solids\n"  
    cginc_content += "struct PlatonicSequence\n"  
    cginc_content += "{\n"  
    cginc_content += f"    uint indices[{SEQUENCE_BUFFER_SIZE}]; // Perfect cycle length of 120 (LCM of all solid sizes)\n"  
    cginc_content += "};\n\n"  
    
    # Define buffer size constants  
    cginc_content += "// Buffer size constants\n"  
    cginc_content += "#define SEQUENCE_COUNT {SEQUENCE_COUNT}\n"  
    cginc_content += f"#define SEQUENCE_BUFFER_SIZE {SEQUENCE_BUFFER_SIZE}\n"  
    cginc_content += f"#define TOTAL_VERTICES {total_vertices}\n\n"  
    
    # Add cycle information comments  
    cginc_content += "// Perfect cycles information (120 is the LCM of 4,6,8,12,20):\n"  
    for solid in solids:  
        cycles = SEQUENCE_BUFFER_SIZE // solid["size"]  
        cginc_content += f"// {solid['name']}: {cycles} complete cycles in buffer ({solid['size']} vertices)\n"  
    cginc_content += "\n"  
    
    for solid in solids:  
        cginc_content += f"#define {solid['name']}_SIZE {solid['size']}\n"  
    cginc_content += "\n"  
    
    # Define the size array  
    cginc_content += "// Solid sizes array\n"  
    cginc_content += "static const uint SOLID_SIZES[5] = {\n"  
    for solid in solids:  
        cginc_content += f"    {solid['size']},  // {solid['name']}\n"  
    cginc_content += "};\n\n"  
    
    # Calculate vertex buffer offsets  
    vertex_offsets = []  
    current_offset = 0  
    for solid in solids:  
        vertex_offsets.append(current_offset)  
        current_offset += solid["size"]  
    
    # Define vertex offsets array  
    cginc_content += "// Vertex offsets for each solid in the unified vertex buffer\n"  
    cginc_content += "static const uint VERTEX_OFFSETS[5] = {\n"  
    for i, offset in enumerate(vertex_offsets):  
        cginc_content += f"    {offset},  // {solids[i]['name']} vertices start offset\n"  
    cginc_content += "};\n\n"  
    
    # Define sequence offsets array  
    cginc_content += "// Sequence offset for each solid in the combined sequence buffer\n"  
    cginc_content += "static const uint SEQUENCE_OFFSETS[5] = {\n"  
    
    seq_offset = 0  
    for i, solid in enumerate(solids):  
        cginc_content += f"    {seq_offset},  // {solid['name']} sequences start offset\n"  
        seq_offset += SEQUENCE_COUNT  
    
    cginc_content += "};\n\n"  
    
    # Create unified vertex buffer  
    cginc_content += "// Unified buffer containing vertices for all platonic solids\n"  
    cginc_content += f"static const float3 PLATONIC_VERTICES[{total_vertices}] = {{\n"  
    
    # Add all vertices from all solids  
    for solid_index, solid in enumerate(solids):  
        cginc_content += f"    // {solid['name']} vertices (offset {vertex_offsets[solid_index]})\n"  
        
        for vertex_index, vertex in enumerate(solid["vertices"]):  
            global_index = vertex_offsets[solid_index] + vertex_index  
            comma = "," if global_index < total_vertices - 1 else ""  
            cginc_content += f"    {vertex}{comma}  // {global_index}\n"  
    
    cginc_content += "};\n\n"  
    
    # Add combined sequence buffer  
    cginc_content += "// Combined platonic sequence buffer with perfect 120-element ping-pong patterns\n"  
    cginc_content += f"static const PlatonicSequence PLATONIC_SEQUENCES[{len(solids) * SEQUENCE_COUNT}] = {{\n"  
    
    # Generate sequences for each solid  
    for solid_index, solid in enumerate(solids):  
        size = solid["size"]  
        sequences = generate_sequence_patterns(size)  
        
        cginc_content += f"    // {solid['name']} sequences (index {solid_index * SEQUENCE_COUNT} to {(solid_index + 1) * SEQUENCE_COUNT - 1})\n"  
        
        for seq_index, seq in enumerate(sequences):  
            # Calculate how many complete cycles fit in the buffer  
            cycles = SEQUENCE_BUFFER_SIZE // size  
            
            cginc_content += f"    // Sequence {seq_index} ({cycles} complete cycles)\n"  
            cginc_content += "    {\n        {"  
            
            # Create perfect ping-pong pattern to fill exactly 120 slots  
            perfect_ping_pong = create_perfect_ping_pong(seq)  
            indices_str = ", ".join(str(idx) for idx in perfect_ping_pong)  
            cginc_content += indices_str  
            
            cginc_content += "}\n    }"  
            
            # Add comma if not the last item  
            is_last = (solid_index == len(solids) - 1) and (seq_index == len(sequences) - 1)  
            comma = "" if is_last else ","  
            cginc_content += f"{comma}\n\n"  
    
    cginc_content += "};\n\n"  
    
    # Add utility function to access vertices using unified sequences and vertex buffer  
    cginc_content += "// Utility function to get a vertex from any solid with sequence pattern\n"  
    cginc_content += "float3 GetPlatonicVertex(uint solidType, uint sequenceIndex, uint vertexIndex)\n"  
    cginc_content += "{\n"  
    cginc_content += "    // Get the solid size and offsets\n"  
    cginc_content += "    uint size = SOLID_SIZES[solidType];\n"  
    cginc_content += "    uint vertexOffset = VERTEX_OFFSETS[solidType];\n"  
    cginc_content += "    uint sequenceOffset = SEQUENCE_OFFSETS[solidType];\n\n"  
    
    cginc_content += "    // Get sequence index with wraparound\n"  
    cginc_content += "    uint seqIdx = sequenceIndex % SEQUENCE_COUNT;\n"  
    cginc_content += "    uint bufferIndex = sequenceOffset + seqIdx;\n\n"  
    
    cginc_content += "    // Get vertex index from the ping-pong sequence (modulo for perfect looping)\n"  
    cginc_content += "    uint idx = vertexIndex % SEQUENCE_BUFFER_SIZE;\n"  
    cginc_content += "    uint vertIdx = PLATONIC_SEQUENCES[bufferIndex].indices[idx];\n\n"  
    
    cginc_content += "    // Return the vertex from the unified buffer\n"  
    cginc_content += "    return PLATONIC_VERTICES[vertexOffset + vertIdx];\n"  
    cginc_content += "}\n\n"  
    
    # Add utility function for sin-like animation using sequence  
    cginc_content += "// Utility function for smooth sinusoidal animation using sequence patterns\n"  
    cginc_content += "float3 GetPlatonicVertexAnimated(uint solidType, uint sequenceIndex, float time, float speed)\n"  
    cginc_content += "{\n"  
    cginc_content += "    // Calculate smooth vertex index based on time\n"  
    cginc_content += "    float animIndex = fmod(time * speed, (float)SEQUENCE_BUFFER_SIZE);\n"  
    cginc_content += "    uint vertexIndex = (uint)animIndex;\n\n"  
    
    cginc_content += "    // Get the vertex\n"  
    cginc_content += "    return GetPlatonicVertex(solidType, sequenceIndex, vertexIndex);\n"  
    cginc_content += "}\n\n"  
    
    # Add individual solid access functions for convenience  
    cginc_content += "// Individual solid access functions\n"  
    for i, solid in enumerate(solids):  
        name = solid["name"]  
        cginc_content += f"float3 Get{name}Vertex(uint sequenceIndex, uint vertexIndex)\n"  
        cginc_content += "{\n"  
        cginc_content += f"    return GetPlatonicVertex({i}, sequenceIndex, vertexIndex);\n"  
        cginc_content += "}\n\n"  
    
    # Add individual animated access functions  
    cginc_content += "// Individual animated access functions\n"  
    for i, solid in enumerate(solids):  
        name = solid["name"]  
        cginc_content += f"float3 Get{name}VertexAnimated(uint sequenceIndex, float time, float speed)\n"  
        cginc_content += "{\n"  
        cginc_content += f"    return GetPlatonicVertexAnimated({i}, sequenceIndex, time, speed);\n"  
        cginc_content += "}\n\n"  
    
    cginc_content += "#endif // PLATONIC_SOLIDS_INCLUDED\n"  
    
    # Write to file in the same directory as the script  
    script_dir = os.path.dirname(os.path.abspath(__file__))  
    output_path = os.path.join(script_dir, "PlatonicSolids.cginc")  
    
    with open(output_path, "w") as f:  
        f.write(cginc_content)  
    
    print(f"Generated PlatonicSolids.cginc with perfect 120-element ping-pong patterns at: {output_path}")  

# Execute the generator  
if __name__ == "__main__":  
    generate_platonic_solids_cginc()  