-- ==========================================
-- TABLE: ServiceType
-- ==========================================
CREATE TABLE IF NOT EXISTS ServiceType (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT
);

-- ==========================================
-- TABLE: Category
-- ==========================================
CREATE TABLE IF NOT EXISTS Category (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT
);

-- ==========================================
-- TABLE: VehicleType
-- ==========================================
CREATE TABLE IF NOT EXISTS VehicleType (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT
);

-- ==========================================
-- TABLE: ServiceOption
-- ==========================================
CREATE TABLE IF NOT EXISTS ServiceOption (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    CategoryId INTEGER,
    FOREIGN KEY (CategoryId) REFERENCES Category(Id)
);

-- ==========================================
-- TABLE: ServiceOptionRelation (Parent/Child hierarchy)
-- ==========================================
CREATE TABLE IF NOT EXISTS ServiceOptionRelation (
    ParentId INTEGER NOT NULL,
    ChildId INTEGER NOT NULL,
    PRIMARY KEY (ParentId, ChildId),
    FOREIGN KEY (ParentId) REFERENCES ServiceOption(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChildId) REFERENCES ServiceOption(Id) ON DELETE CASCADE
);

-- ==========================================
-- TABLE: ServiceOptionServiceType (Many-to-Many)
-- ==========================================
CREATE TABLE IF NOT EXISTS ServiceOptionServiceType (
    ServiceOptionId INTEGER NOT NULL,
    ServiceTypeId INTEGER NOT NULL,
    PRIMARY KEY (ServiceOptionId, ServiceTypeId),
    FOREIGN KEY (ServiceOptionId) REFERENCES ServiceOption(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceTypeId) REFERENCES ServiceType(Id) ON DELETE CASCADE
);

-- ==========================================
-- TABLE: VehicleTypeServiceOption (Many-to-Many)
-- ==========================================
CREATE TABLE IF NOT EXISTS VehicleTypeServiceOption (
    VehicleTypeId INTEGER NOT NULL,
    ServiceOptionId INTEGER NOT NULL,
    PRIMARY KEY (VehicleTypeId, ServiceOptionId),
    FOREIGN KEY (VehicleTypeId) REFERENCES VehicleType(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceOptionId) REFERENCES ServiceOption(Id) ON DELETE CASCADE
);








-- ==========================================
-- INSERTS: Category
-- ==========================================
INSERT OR IGNORE INTO Category (Name, Description)
VALUES 
('Service', 'All service-related options for vehicles'),
('Ownership', 'Options related to vehicle ownership and administration');

-- ==========================================
-- INSERTS: ServiceType
-- ==========================================
INSERT OR IGNORE INTO ServiceType (Name, Description)
VALUES
('Maintenance', 'Routine maintenance services to keep vehicles in optimal condition'),
('Replacement', 'Replacement of faulty or worn vehicle parts'),
('Inspection', 'Inspection and certification services for vehicle compliance'),
('Adjustment', 'Fine-tuning or adjustment of vehicle components for proper function'),
('Tune', 'Engine or system tuning to improve performance and efficiency'),
('WOF', 'Warrant of Fitness inspection for ensuring vehicle roadworthiness'),
('Registration', 'Vehicle registration services for legal compliance and renewal'),
('Certification', 'Vehicle modification or compliance certification services');

-- ==========================================
-- INSERTS: VehicleType
-- ==========================================
INSERT OR IGNORE INTO VehicleType (Name, Description)
VALUES
('Car', 'Standard passenger vehicles'),
('Truck', 'Light and heavy trucks'),
('Motorbike', 'Motorcycles and scooters');

-- ==========================================
-- INSERTS: ServiceOption
-- ==========================================
-- CategoryId is fetched inline via a subquery for 'Service' category
INSERT OR IGNORE INTO ServiceOption (Name, Description, CategoryId)
SELECT 'Engine', 'Engine components and maintenance services', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel System', 'Fuel delivery and filtration components', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel Tank', 'Fuel storage tank inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel Pump', 'Fuel or oil pump maintenance and replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel Filter', 'Fuel or oil filter replacement and cleaning', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel stop', 'Fuel line shutoff or valve maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Carburetor/ Fuel injection', 'Tuning, cleaning, or repairing carburetor or fuel injection system', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Float chamber', 'Maintenance of carburetor float chamber assembly', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Float & needle valve', 'Inspection or replacement of float and needle valve', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Venturi', 'Venturi cleaning or performance tuning', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Throttle valve', 'Throttle body or valve adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Jet system', 'Carburetor jet tuning or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Choke', 'Choke system operation and adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Air filter', 'Air filter inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Fuel line', 'Fuel line replacement or leak check', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Top end', 'Maintenance of cylinder head and valve components', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Timing adjustment', 'Adjusting ignition or camshaft timing', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Casing', 'Inspection or replacement of engine casing', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Piston', 'Piston inspection, cleaning, or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Timing chain', 'Timing chain replacement or tension adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Manifold', 'Intake or exhaust manifold maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Valves', 'Valve clearance adjustment and inspection', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Shims', 'Shim adjustment for valve clearance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Rockers', 'Rocker arm inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Cylinder head', 'Cylinder head inspection, cleaning, or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Bottom end', 'Crankshaft and lower engine assembly maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Timing chain adjuster', 'Inspection or replacement of chain tensioner', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Barrel', 'Cylinder barrel reboring or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Con rod', 'Connecting rod inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Crank case', 'Crankcase cleaning or sealing maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Oil pump', 'Oil pump inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Oil filter', 'Oil filter replacement or upgrade', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Gaskets', 'Replacement of worn or leaking gaskets', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Starting system', 'Inspection of starter motor and related wiring', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Gear lever', 'Gear shift lever replacement or adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Cam shaft', 'Camshaft replacement or timing adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Spark plug', 'Spark plug replacement and gap setting', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Stator', 'Stator inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Mount', 'Inspection or replacement of engine or exhaust mounts', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Transmission', 'Transmission inspection, rebuild, or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Clutch', 'Clutch assembly inspection and replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Clutch baskets', 'Replacement or resurfacing of clutch baskets', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Clutch discs', 'Inspection or replacement of clutch discs', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Chassis/ Frame', 'Frame or chassis inspection and maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Sub-frame', 'Sub-frame alignment and repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Paint', 'Paint touch-up or full repaint', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Bracing/ reinforcement', 'Frame strengthening or reinforcement work', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Power coating', 'Powder coating for protection and aesthetics', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Frame sliders', 'Installation or replacement of frame sliders', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Decals', 'Application or replacement of vehicle decals', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Plastics', 'Repair or replacement of fairings and plastic panels', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Electrical', 'Diagnosis and repair of electrical systems', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Gauge', 'Instrument cluster or gauge repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Battery', 'Battery replacement and charging system check', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Alternator', 'Alternator inspection and replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Regulator', 'Voltage regulator testing or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Starter motor', 'Starter motor repair or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Solenoid', 'Starter solenoid inspection and testing', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Headlight', 'Headlight bulb or housing replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Brake light', 'Brake light bulb or wiring repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Indicators', 'Turn signal bulb or relay repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'License plate light', 'License plate light replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Ignition switch', 'Ignition switch replacement or repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'ECU', 'Engine control unit testing and reprogramming', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Horn', 'Horn replacement or wiring check', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Oxygen sensor', 'O2 sensor inspection and replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Suspension', 'Forks and shocks maintenance or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Forks', 'Front fork seal replacement or tuning', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Bars', 'Handlebar alignment or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Clutch lever', 'Clutch lever replacement or adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Brake lever', 'Brake lever replacement or adjustment', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Throttle', 'Throttle cable adjustment or lubrication', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Grip', 'Handle grip replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Triple clamp', 'Triple clamp alignment or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Bearings', 'Bearing inspection and replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Damper', 'Steering or suspension damper service', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Wheels', 'Wheel inspection, alignment, or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Spokes', 'Spoke tension adjustment or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Tires', 'Tire replacement or pressure check', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Chain', 'Drive chain cleaning, tensioning, or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Tensioner', 'Chain or belt tensioner inspection', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Sprockets', 'Front or rear sprocket replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Brakes', 'Brake system maintenance and repair', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Brake Pads', 'Brake pad replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Brake Disc', 'Brake disc inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Lines', 'Brake line inspection or bleeding', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'ABS', 'ABS sensor or module testing', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Calipers', 'Brake caliper rebuild or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Bleed', 'Brake fluid bleeding procedure', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Master cylinder', 'Master cylinder rebuild or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Slave cylinder', 'Slave cylinder rebuild or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Intake', 'Air intake system upgrade or maintenance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Exhaust', 'Exhaust system maintenance or upgrade', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Header', 'Exhaust header inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Mid-pipe', 'Mid-pipe replacement or upgrade', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Muffler', 'Muffler replacement or modification', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Catalytic converter', 'Catalytic converter inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Mounts & hangers', 'Mount or hanger replacement for exhaust or accessories', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Heat shield', 'Heat shield inspection or replacement', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Accessories', 'Additional accessories or upgrades', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Extra', 'Miscellaneous services not categorized above', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Oil Change', 'Drain and replace engine oil and oil filter to maintain lubrication and performance', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Cooling System', 'System responsible for regulating engine temperature and preventing overheating', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Thermostat', 'Controls coolant flow between the engine and radiator to maintain optimal temperature', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Radiator', 'Dissipates heat from engine coolant; may require repair or replacement if leaking or blocked', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Coolant Flush', 'Flush and replace engine coolant to prevent overheating and corrosion in the cooling system', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Ownership', 'Options related to vehicle ownership and administration', Id FROM Category WHERE Name = 'Ownership'
UNION ALL
SELECT 'Spacers', 'Check, clean, and replace worn wheel spacers to maintain proper alignment.', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Upper shock', 'Service the upper shock mount including inspection, lubrication, and bushings.', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Lower shock', 'Service the lower shock mount, including cleaning, lubrication, and hardware checks.', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Linkage', 'Full linkage service including bearings, seals, lubrication, and movement check.', Id FROM Category WHERE Name = 'Service'
UNION ALL
SELECT 'Countershaft', 'Inspect, clean, and replace countershaft seals or sprocket components as needed.', Id FROM Category WHERE Name = 'Service';



-- ====== Add Service Types for Each ======

-- Ownership: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'WOF') 
FROM ServiceOption 
WHERE Name = 'Ownership';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Registration') 
FROM ServiceOption 
WHERE Name = 'Ownership';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Certification') 
FROM ServiceOption 
WHERE Name = 'Ownership';




-- ===== Level 1: Engine =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Engine'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Oil Change');

-- ===== Level 2: Oil Change - Add Service Types =====
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance')
FROM ServiceOption
WHERE Name = 'Oil Change';


-- ===== Level 1: Fuel System =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Engine'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Fuel System');

-- Level 2: Fuel System children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Fuel System'),
    Id
FROM ServiceOption
WHERE Name IN ('Fuel Tank', 'Fuel Pump', 'Fuel Filter', 'Fuel stop');

-- ====== Add Service Types for Each ======

-- Tank: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Fuel Tank';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Fuel Tank';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Fuel Tank';

-- Pump: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Fuel Pump';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Fuel Pump';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Fuel Pump';

-- Filter: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Fuel Filter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Fuel Filter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Fuel Filter';

-- Fuel stop: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Fuel stop';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Fuel stop';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Fuel stop';



-- ===== Level 1: Carburetor / Fuel Injection =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Engine'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Carburetor/ Fuel injection');

-- Level 2: Carburetor / Fuel Injection children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Carburetor/ Fuel injection'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Float chamber',
    'Float & needle valve',
    'Venturi',
    'Throttle valve',
    'Jet system',
    'Choke',
    'Air filter',
    'Fuel line'
);

-- ====== Add Service Types for Each ======

-- Float chamber: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Float chamber';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Float chamber';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Float chamber';

-- Float & needle valve: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Float & needle valve';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Float & needle valve';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Float & needle valve';

-- Venturi: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Venturi';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Venturi';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Venturi';

-- Throttle valve: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Throttle valve';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Throttle valve';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Throttle valve';

-- Jet system: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Jet system';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Jet system';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Jet system';

-- Choke: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Choke';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Choke';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Choke';

-- Air filter: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Air filter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Air filter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Air filter';

-- Fuel line: Add Service Types
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Fuel line';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Fuel line';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Fuel line';




-- ===== Level 1: Top End =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Engine'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Top end');

-- Level 2: Top End children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Top end'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Timing adjustment',
    'Casing',
    'Piston',
    'Timing chain',
    'Manifold',
    'Valves',
    'Shims',
    'Rockers',
    'Cylinder head'
);

-- ====== Add Service Types for Each ======

-- Timing adjustment
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Timing adjustment';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Timing adjustment';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Timing adjustment';

-- Casing
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Casing';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Casing';

-- Piston
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Piston';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Piston';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Piston';

-- Timing chain
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Timing chain';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Timing chain';

-- Manifold
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Manifold';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Manifold';

-- Valves
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Valves';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Valves';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Valves';

-- Shims
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Shims';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Shims';

-- Rockers
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Rockers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Rockers';

-- Cylinder head
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Cylinder head';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Cylinder head';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Cylinder head';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Tune') FROM ServiceOption WHERE Name = 'Cylinder head';




-- ===== Level 1: Bottom End =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Engine'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Bottom end');

-- Level 2: Bottom End children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Bottom end'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Timing chain adjuster',
    'Barrel',
    'Con rod',
    'Crank case',
    'Oil pump',
    'Oil filter',
    'Gaskets',
    'Starting system',
    'Gear lever',
    'Cam shaft',
    'Spark plug',
    'Stator',
    'Mount'
);

-- ====== Add Service Types for Each Bottom End child ======

-- Timing chain adjuster
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Timing chain adjuster';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Timing chain adjuster';

-- Barrel
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Barrel';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Barrel';

-- Con rod
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Con rod';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Con rod';

-- Crank case
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Crank case';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Crank case';

-- Oil pump
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Oil pump';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Oil pump';

-- Oil filter
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Oil filter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Oil filter';

-- Gaskets
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Gaskets';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Gaskets';

-- Starting system
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Starting system';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Starting system';

-- Gear lever
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Gear lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Gear lever';

-- Cam shaft
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Cam shaft';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Cam shaft';

-- Spark plug
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Spark plug';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Spark plug';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Spark plug';

-- Stator
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Stator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Stator';

-- Mount
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Mount';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Mount';


-- ==========================================
-- TRANSMISSION HIERARCHY
-- ==========================================

-- Link Transmission to its children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Transmission'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Clutch',
    'Clutch baskets',
    'Clutch discs',
    'Countershaft'
);

-- ====== Add Service Types for Each Transmission child ======

-- Clutch
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Clutch';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Clutch';
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Clutch';

-- Clutch baskets
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Clutch baskets';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Clutch baskets';
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Clutch baskets';

-- Clutch discs
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Clutch discs';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Clutch discs';
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Clutch discs';

-- Countershaft discs
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Countershaft';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Countershaft';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Countershaft';


-- ==========================================
-- CHASSIS / FRAME HIERARCHY
-- ==========================================

-- Sub-frame
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Chassis/ Frame'),
    Id
FROM ServiceOption
WHERE Name = 'Sub-frame';

-- Sub-frame children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Sub-frame'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Paint',
    'Bracing/ reinforcement',
    'Power coating',
    'Frame sliders'
);

-- ====== Add Service Types for Each Sub-frame child ======

-- Paint
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Paint';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Paint';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Paint';

-- Bracing/ reinforcement
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Bracing/ reinforcement';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Bracing/ reinforcement';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Bracing/ reinforcement';

-- Power coating
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Power coating';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Power coating';

-- Frame sliders
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Frame sliders';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Frame sliders';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') 
FROM ServiceOption WHERE Name = 'Frame sliders';


-- Frame children (not under sub-frame)
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Chassis/ Frame'),
    Id
FROM ServiceOption
WHERE Name IN (
    'Decals',
    'Plastics'
);

-- ====== Add Service Types for Each Frame child ======

-- Decals
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Decals';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Decals';

-- Plastics
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') 
FROM ServiceOption WHERE Name = 'Plastics';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') 
FROM ServiceOption WHERE Name = 'Plastics';


-- ==========================================
-- ELECTRICAL HIERARCHY
-- ==========================================

-- Insert parent-child relations
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Electrical'), Id
FROM ServiceOption
WHERE Name IN (
    'Gauge',
    'Battery',
    'Alternator',
    'Regulator',
    'Starter motor',
    'Solenoid',
    'Headlight',
    'Brake light',
    'Indicators',
    'License plate light',
    'Ignition switch',
    'ECU',
    'Horn',
    'Oxygen sensor'
);

-- ====== Add Service Types for Each Electrical child ======

-- Loop replacement: insert ServiceType relations individually using inline subqueries

-- Gauge
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Gauge';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Gauge';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Gauge';

-- Battery
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Battery';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Battery';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Battery';

-- Alternator
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Alternator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Alternator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Alternator';

-- Regulator
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Regulator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Regulator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Regulator';

-- Starter motor
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Starter motor';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Starter motor';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Starter motor';

-- Solenoid
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Solenoid';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Solenoid';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Solenoid';

-- Headlight
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Headlight';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Headlight';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Headlight';

-- Brake light
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Brake light';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Brake light';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Brake light';

-- Indicators
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Indicators';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Indicators';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Indicators';

-- License plate light
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'License plate light';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'License plate light';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'License plate light';

-- Ignition switch
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Ignition switch';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Ignition switch';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Ignition switch';

-- ECU
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'ECU';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'ECU';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'ECU';

-- Horn
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Horn';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Horn';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Horn';

-- Oxygen sensor
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Oxygen sensor';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Oxygen sensor';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Oxygen sensor';


-- ==========================================
-- SUSPENSION HIERARCHY
-- ==========================================

-- Forks directly under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Forks';

-- Add Service Types for Forks
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Forks';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Forks';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Forks';


-- Upper shock directly under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Upper shock';

-- Add Service Types for Upper shock
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Upper shock';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Upper shock';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Upper shock';



-- Lower shock directly under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Lower shock';

-- Add Service Types for Lower shock
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Lower shock';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Lower shock';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Lower shock';


-- Linkage directly under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Linkage';

-- Add Service Types for Linkage
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Linkage';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Linkage';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Linkage';




-- Bars under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Bars';

-- Bars children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Bars'), Id
FROM ServiceOption
WHERE Name IN (
    'Clutch lever',
    'Brake lever',
    'Throttle',
    'Grip',
    'Triple clamp',
    'Bearings',
    'Damper'
);

-- Add Service Types for each Bars child

-- Clutch lever
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Clutch lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Clutch lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Clutch lever';

-- Brake lever
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Brake lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Brake lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Brake lever';

-- Throttle
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Throttle';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Throttle';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Throttle';

-- Grip
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Grip';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Grip';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Grip';

-- Triple clamp
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Triple clamp';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Triple clamp';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Triple clamp';

-- Bearings
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Bearings';

-- Damper
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Damper';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Damper';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Damper';


-- ==========================================
-- SUSPENSION HIERARCHY
-- ==========================================

-- Forks directly under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Forks';

-- Add Service Types for Forks
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Forks';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Forks';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Forks';

-- Bars under Suspension
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Suspension'), Id
FROM ServiceOption
WHERE Name = 'Bars';

-- Bars children
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Bars'), Id
FROM ServiceOption
WHERE Name IN (
    'Clutch lever',
    'Brake lever',
    'Throttle',
    'Grip',
    'Triple clamp',
    'Bearings',
    'Damper'
);

-- Add Service Types for each Bars child

-- Clutch lever
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Clutch lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Clutch lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Clutch lever';

-- Brake lever
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Brake lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Brake lever';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Brake lever';

-- Throttle
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Throttle';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Throttle';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Throttle';

-- Grip
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Grip';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Grip';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Grip';

-- Triple clamp
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Triple clamp';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Triple clamp';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Triple clamp';

-- Bearings
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Bearings';

-- Damper
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Damper';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Damper';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Damper';


-- ==========================================
-- Breaks SUB-HIERARCHY
-- ==========================================

-- Insert child ServiceOptions under Breaks
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT Id, (SELECT Id FROM ServiceOption WHERE Name = 'Brake Pads')
FROM ServiceOption
WHERE Name = 'Brakes';

INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT Id, (SELECT Id FROM ServiceOption WHERE Name = 'Brake Disc')
FROM ServiceOption
WHERE Name = 'Brakes';

-- Add Service Types for Pads
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance')
FROM ServiceOption
WHERE Name = 'Brake Pads';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement')
FROM ServiceOption
WHERE Name = 'Brake Pads';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection')
FROM ServiceOption
WHERE Name = 'Brake Pads';

-- Add Service Types for Disc
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance')
FROM ServiceOption
WHERE Name = 'Brake Disc';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement')
FROM ServiceOption
WHERE Name = 'Brake Disc';

INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection')
FROM ServiceOption
WHERE Name = 'Brake Disc';


-- ==========================================
-- Wheels SUB-HIERARCHY
-- ==========================================

-- Add children under Wheels
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Wheels'), Id
FROM ServiceOption
WHERE Name IN ('Tires', 'Chain', 'Tensioner', 'Sprockets', 'Bearings', 'Spacers');

-- Tires
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Tires';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Tires';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Tires';

-- Chain
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Chain';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Chain';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Chain';

-- Tensioner
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Tensioner';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Tensioner';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Tensioner';

-- Sprockets
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Sprockets';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Sprockets';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Sprockets';

-- Bearings
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Bearings';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Bearings';

-- Spacers
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Spacers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Spacers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Spacers';


-- ==========================================
-- Lines SUB-HIERARCHY
-- ==========================================

-- Add children under Lines
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Lines'), Id
FROM ServiceOption
WHERE Name IN ('ABS', 'Calipers', 'Bleed', 'Master cylinder', 'Slave cylinder');

-- Add Service Types for each Lines child

-- ABS
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'ABS';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'ABS';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'ABS';

-- Calipers
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Calipers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Calipers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Calipers';

-- Bleed
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Bleed';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Bleed';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Bleed';

-- Master cylinder
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Master cylinder';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Master cylinder';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Master cylinder';

-- Slave cylinder
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Slave cylinder';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Slave cylinder';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Slave cylinder';



-- ==========================================
-- Exhaust SUB-HIERARCHY
-- ==========================================

-- Add children under Exhaust
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Exhaust'), Id
FROM ServiceOption
WHERE Name IN (
    'Header',
    'Mid-pipe',
    'Muffler',
    'Catalytic converter',
    'Mounts & hangers',
    'Heat shield'
);

-- Add Service Types for each Exhaust child

-- Header
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Header';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Header';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Header';

-- Mid-pipe
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Mid-pipe';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Mid-pipe';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Mid-pipe';

-- Muffler
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Muffler';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Muffler';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Muffler';

-- Catalytic converter
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Catalytic converter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Catalytic converter';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Catalytic converter';

-- Mounts & hangers
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Mounts & hangers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Mounts & hangers';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Mounts & hangers';

-- Heat shield
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Heat shield';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Heat shield';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Heat shield';




-- ===== Level 1: Cooling System =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Cooling System'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Thermostat');

-- ====== Add Service Types for Thermostat ======
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Thermostat';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Thermostat';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Thermostat';


-- ===== Level 1: Cooling System =====
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT 
    (SELECT Id FROM ServiceOption WHERE Name = 'Cooling System'),
    (SELECT Id FROM ServiceOption WHERE Name = 'Radiator');

-- ====== Add Service Types for Radiator ======
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Radiator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Radiator';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Radiator';


-- ===== Level 2: Cooling System / Radiator Child =====
-- Link Coolant Flush under Radiator (or Cooling System, if preferred)
INSERT OR IGNORE INTO ServiceOptionRelation (ParentId, ChildId)
SELECT (SELECT Id FROM ServiceOption WHERE Name = 'Radiator'), Id
FROM ServiceOption
WHERE Name = 'Coolant Flush';

-- ====== Add Service Types for Coolant Flush ======
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Maintenance') FROM ServiceOption WHERE Name = 'Coolant Flush';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Replacement') FROM ServiceOption WHERE Name = 'Coolant Flush';
INSERT OR IGNORE INTO ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
SELECT Id, (SELECT Id FROM ServiceType WHERE Name = 'Inspection') FROM ServiceOption WHERE Name = 'Coolant Flush';





-- Link each ServiceOption individually to Motorbike

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Engine';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Transmission';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Chassis/ Frame';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Electrical';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Suspension';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Wheels';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Intake';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Exhaust';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Accessories';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Extra';

INSERT OR IGNORE INTO VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
SELECT (SELECT Id FROM VehicleType WHERE Name = 'Motorbike'), Id 
FROM ServiceOption WHERE Name = 'Cooling System';

SELECT 
  (SELECT Id FROM VehicleType WHERE Name = 'Motorbike') AS VehicleTypeId,
  Id AS ServiceOptionId
FROM ServiceOption 
WHERE Name = 'Brakes';
