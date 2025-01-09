import os
from pathlib import Path
import re

class MediatrCrudGenerator:
    def __init__(self, solution_path):
        self.solution_path = Path(solution_path)
        self.entities_path = self.find_entities_folder()
        
    def find_entities_folder(self):
        """Find the Entities folder in the Domain project"""
        for path in self.solution_path.rglob('*Domain/Entities'):
            if path.is_dir():
                return path
        raise Exception("Could not find Domain/Entities folder in solution")
        
    def get_entities(self):
        """Scan the entities directory and return all .cs files"""
        if not self.entities_path:
            return []
            
        entities = []
        print(f"Scanning for entities in: {self.entities_path}")
        
        try:
            for file in self.entities_path.glob("*.cs"):
                if not file.name.startswith('I') and file.name != "BaseEntity.cs":
                    print(f"Found entity file: {file.name}")
                    entities.append(file.stem)
        except Exception as e:
            print(f"Error scanning entities: {e}")
            
        return entities

    def parse_entity_properties(self, entity_name):
        """Parse the C# entity file to extract properties"""
        properties = []
        entity_path = self.entities_path / f"{entity_name}.cs"
        
        try:
            with open(entity_path, 'r', encoding='utf-8') as file:
                content = file.read()
                
                prop_pattern = r"(?:^\s*\[.*?\]\s*)?public\s+(\w+(?:<.*?>)?)\s+(\w+)\s*{\s*get;\s*(?:private\s+)?set;\s*}"
                matches = re.finditer(prop_pattern, content, re.MULTILINE)
                
                for match in matches:
                    prop_type = match.group(1)
                    prop_name = match.group(2)
                    properties.append({
                        'type': prop_type,
                        'name': prop_name
                    })
                    print(f"Found property: {prop_name} ({prop_type})")
                    
        except Exception as e:
            print(f"Error parsing entity {entity_name}: {e}")
                
        return properties

    def generate_command(self, entity_name, properties, command_type):
        """Generate command class based on template"""
        templates = {
            'create': '''using MediatR;

public class Create{entity_name}Command : IRequest<int>
{{
{properties}
}}

public class Create{entity_name}CommandHandler : IRequestHandler<Create{entity_name}Command, int>
{{
    private readonly IApplicationDbContext _context;
    
    public Create{entity_name}CommandHandler(IApplicationDbContext context)
    {{
        _context = context;
    }}
    
    public async Task<int> Handle(Create{entity_name}Command request, CancellationToken cancellationToken)
    {{
        var entity = new {entity_name}
        {{
{mapping}
        }};
        
        _context.{entity_name}s.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity.Id;
    }}
}}''',
            'update': '''using MediatR;

public class Update{entity_name}Command : IRequest<Unit>
{{
    public int Id {{ get; set; }}
{properties}
}}

public class Update{entity_name}CommandHandler : IRequestHandler<Update{entity_name}Command, Unit>
{{
    private readonly IApplicationDbContext _context;
    
    public Update{entity_name}CommandHandler(IApplicationDbContext context)
    {{
        _context = context;
    }}
    
    public async Task<Unit> Handle(Update{entity_name}Command request, CancellationToken cancellationToken)
    {{
        var entity = await _context.{entity_name}s.FindAsync(request.Id);
        
        if (entity == null)
            throw new NotFoundException(nameof({entity_name}), request.Id);
            
{mapping}
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }}
}}''',
            'delete': '''using MediatR;

public class Delete{entity_name}Command : IRequest<Unit>
{{
    public int Id {{ get; set; }}
}}

public class Delete{entity_name}CommandHandler : IRequestHandler<Delete{entity_name}Command, Unit>
{{
    private readonly IApplicationDbContext _context;
    
    public Delete{entity_name}CommandHandler(IApplicationDbContext context)
    {{
        _context = context;
    }}
    
    public async Task<Unit> Handle(Delete{entity_name}Command request, CancellationToken cancellationToken)
    {{
        var entity = await _context.{entity_name}s.FindAsync(request.Id);
        
        if (entity == null)
            throw new NotFoundException(nameof({entity_name}), request.Id);
            
        _context.{entity_name}s.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }}
}}'''
        }
        
        props = []
        mappings = []
        for prop in properties:
            if command_type == 'create' or (command_type == 'update' and prop['name'] != 'Id'):
                props.append(f"    public {prop['type']} {prop['name']} {{ get; set; }}")
                mappings.append(f"            {prop['name']} = request.{prop['name']}")
        
        properties_str = '\n'.join(props)
        mappings_str = '\n'.join(mappings)
        
        return templates[command_type].format(
            entity_name=entity_name,
            properties=properties_str,
            mapping=mappings_str
        )

    def generate_query(self, entity_name):
        """Generate query classes for getting all entities and getting by id"""
        get_all_template = '''using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

public class GetAll{entity_name}sQuery : IRequest<IEnumerable<{entity_name}Dto>> {{ }}

public class GetAll{entity_name}sQueryHandler : IRequestHandler<GetAll{entity_name}sQuery, IEnumerable<{entity_name}Dto>>
{{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetAll{entity_name}sQueryHandler(IApplicationDbContext context, IMapper mapper)
    {{
        _context = context;
        _mapper = mapper;
    }}
    
    public async Task<IEnumerable<{entity_name}Dto>> Handle(GetAll{entity_name}sQuery request, CancellationToken cancellationToken)
    {{
        return await _context.{entity_name}s
            .ProjectTo<{entity_name}Dto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }}
}}'''
        
        get_by_id_template = '''using MediatR;
using AutoMapper;

public class Get{entity_name}ByIdQuery : IRequest<{entity_name}Dto>
{{
    public int Id {{ get; set; }}
}}

public class Get{entity_name}ByIdQueryHandler : IRequestHandler<Get{entity_name}ByIdQuery, {entity_name}Dto>
{{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public Get{entity_name}ByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {{
        _context = context;
        _mapper = mapper;
    }}
    
    public async Task<{entity_name}Dto> Handle(Get{entity_name}ByIdQuery request, CancellationToken cancellationToken)
    {{
        var entity = await _context.{entity_name}s.FindAsync(request.Id);
        
        if (entity == null)
            throw new NotFoundException(nameof({entity_name}), request.Id);
            
        return _mapper.Map<{entity_name}Dto>(entity);
    }}
}}'''
        
        return {
            'get_all': get_all_template.format(entity_name=entity_name),
            'get_by_id': get_by_id_template.format(entity_name=entity_name)
        }

    def generate_dto(self, entity_name, properties):
        """Generate DTO class"""
        props = []
        for prop in properties:
            props.append(f"    public {prop['type']} {prop['name']} {{ get; set; }}")
            
        dto_template = '''public class {entity_name}Dto
{{
{properties}
}}'''
        
        return dto_template.format(
            entity_name=entity_name,
            properties='\n'.join(props)
        )

    def generate_crud(self, entity_name, output_path):
        """Generate all CRUD operations for an entity"""
        properties = self.parse_entity_properties(entity_name)
        
        base_path = Path(output_path) / entity_name / "Features" / f"{entity_name}s"
        
        # Commands
        commands_path = base_path / "Commands"
        commands_path.mkdir(parents=True, exist_ok=True)
        
        for command_type in ['create', 'update', 'delete']:
            command_dir = commands_path / f"{command_type.capitalize()}{entity_name}"
            command_dir.mkdir(exist_ok=True)
            
            command_content = self.generate_command(entity_name, properties, command_type)
            command_file = command_dir / f"{command_type.capitalize()}{entity_name}Command.cs"
            
            with open(command_file, 'w') as f:
                f.write(command_content)
        
        # Queries
        queries_path = base_path / "Queries"
        queries_path.mkdir(parents=True, exist_ok=True)
        
        queries = self.generate_query(entity_name)
        
        for query_type, content in queries.items():
            query_dir = queries_path / f"{query_type.replace('_', '').title()}"
            query_dir.mkdir(exist_ok=True)
            
            query_file = query_dir / f"{query_type.replace('_', '').title()}.cs"
            
            with open(query_file, 'w') as f:
                f.write(content)
        
        # DTO
        dto_content = self.generate_dto(entity_name, properties)
        dto_file = queries_path / f"{entity_name}Dto.cs"
        
        with open(dto_file, 'w') as f:
            f.write(dto_content)

def find_solution_root():
    """Find the solution root by looking for .sln file"""
    current_dir = Path(__file__).resolve().parents[2]  # Go up two levels from scripts/crud
    
    # Look in current directory and up to 3 levels up
    for _ in range(4):
        sln_files = list(current_dir.glob('*.sln'))
        if sln_files:
            return current_dir
        current_dir = current_dir.parent
        
    raise Exception("Could not find solution file (.sln)")

if __name__ == "__main__":
    try:
        solution_root = find_solution_root()
        print(f"Found solution root at: {solution_root}")
        
        generator = MediatrCrudGenerator(solution_root)
        entities = generator.get_entities()
        
        if not entities:
            print("\nNo entities found! Please check:")
            print("1. The path to your entities folder is correct")
            print("2. The entities folder contains .cs files")
            print("3. The files don't start with 'I' (interface)")
            print(f"\nLooked in: {generator.entities_path}")
        else:
            print("\nAvailable entities:")
            for i, entity in enumerate(entities, 1):
                print(f"{i}. {entity}")
            
            selection = int(input("\nSelect an entity number to generate CRUD operations: ")) - 1
            if 0 <= selection < len(entities):
                entity_name = entities[selection]
                output_path = solution_root / "Output"
                
                generator.generate_crud(entity_name, output_path)
                print(f"\nCRUD operations generated for {entity_name} in the {output_path} directory.")
            else:
                print("Invalid selection!")
                
    except Exception as e:
        print(f"\nError: {e}")
        print("\nPlease make sure you're running this script from within the scripts/crud directory.")