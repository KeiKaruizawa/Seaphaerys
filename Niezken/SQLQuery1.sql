DELETE FROM Ships;
DBCC CHECKIDENT ('Ships', RESEED, 0);

INSERT INTO Ships (Name, Route, Price, Description, Image, RouteId, DepartureTime) VALUES

(N'MV St. Nicholas',  N'Manila to Cebu',    N'₱1,500', N'A modern and spacious passenger vessel offering premium cabins, a full dining area, and entertainment facilities. Ideal for overnight inter-island voyages between Manila and Cebu.', N'ship1.jpg', 1, N'8:00 PM'),
(N'MV St. Nicholas',  N'Cebu to Manila',    N'₱1,500', N'A modern and spacious passenger vessel offering premium cabins, a full dining area, and entertainment facilities. Ideal for overnight inter-island voyages between Manila and Cebu.', N'ship1.jpg', 1, N'6:00 PM'),

(N'MV St. Joseph',    N'Cebu to Davao',     N'₱1,800', N'Designed for long-distance sea travel, MV St. Joseph features wide deck spaces, air-conditioned cabins, and a fully equipped galley serving hot meals throughout the journey.', N'ship2.jpg', 2, N'7:00 PM'),
(N'MV St. Joseph',    N'Davao to Cebu',     N'₱1,800', N'Designed for long-distance sea travel, MV St. Joseph features wide deck spaces, air-conditioned cabins, and a fully equipped galley serving hot meals throughout the journey.', N'ship2.jpg', 2, N'5:00 PM'),

(N'MV St. Augustine', N'Manila to Bacolod', N'₱1,600', N'A reliable and fast ferry with ergonomic premium seating, onboard Wi-Fi, and a dedicated family lounge. Perfect for the Manila-Bacolod overnight route.', N'ship3.jpg', 3, N'9:00 PM'),
(N'MV St. Augustine', N'Bacolod to Manila', N'₱1,600', N'A reliable and fast ferry with ergonomic premium seating, onboard Wi-Fi, and a dedicated family lounge. Perfect for the Manila-Bacolod overnight route.', N'ship3.jpg', 3, N'7:00 PM'),

(N'MV St. Leo',       N'Cebu to Iloilo',    N'₱1,200', N'An affordable yet comfortable vessel connecting Cebu and Iloilo. Offers clean economy cabins, a snack bar, and open-air deck seating with panoramic sea views.', N'ship4.jpg', 4, N'10:00 AM'),
(N'MV St. Leo',       N'Iloilo to Cebu',    N'₱1,200', N'An affordable yet comfortable vessel connecting Cebu and Iloilo. Offers clean economy cabins, a snack bar, and open-air deck seating with panoramic sea views.', N'ship4.jpg', 4, N'2:00 PM'),

(N'MV St. John Paul', N'Manila to Palawan', N'₱2,200', N'Our flagship luxury vessel serving the Manila-Palawan corridor. Boasts premium suite cabins, a full-service restaurant, spa facilities, and a children''s play area.', N'ship5.jpg', 5, N'6:00 PM'),
(N'MV St. John Paul', N'Palawan to Manila', N'₱2,200', N'Our flagship luxury vessel serving the Manila-Palawan corridor. Boasts premium suite cabins, a full-service restaurant, spa facilities, and a children''s play area.', N'ship5.jpg', 5, N'8:00 AM'),

(N'MV St. Francis',   N'Cebu to Bacolod',   N'₱1,100', N'An efficient and well-maintained inter-island ferry covering the Cebu-Bacolod route. Features comfortable reclining seats, a cafeteria, and a spacious cargo deck.', N'ship6.jpg', 6, N'11:00 PM'),
(N'MV St. Francis',   N'Bacolod to Cebu',   N'₱1,100', N'An efficient and well-maintained inter-island ferry covering the Cebu-Bacolod route. Features comfortable reclining seats, a cafeteria, and a spacious cargo deck.', N'ship6.jpg', 6, N'9:00 AM'),

(N'MV St. Peter',     N'Iloilo to Manila',  N'₱1,900', N'Equipped with modern navigation systems and a full passenger manifest capacity, MV St. Peter is the go-to vessel for the Iloilo-Manila route with overnight cabin options.', N'ship7.jpg', 7, N'6:00 PM'),
(N'MV St. Peter',     N'Manila to Iloilo',  N'₱1,900', N'Equipped with modern navigation systems and a full passenger manifest capacity, MV St. Peter is the go-to vessel for the Iloilo-Manila route with overnight cabin options.', N'ship7.jpg', 7, N'8:00 PM'),

(N'MV St. Benedict',  N'Cebu to Palawan',   N'₱2,000', N'A high-capacity ship purpose-built for the Cebu-Palawan sea corridor. Offers multiple cabin classes, a sun deck, and onboard retail shops for a pleasant long-haul journey.', N'ship8.jpg', 8, N'7:00 PM'),
(N'MV St. Benedict',  N'Palawan to Cebu',   N'₱2,000', N'A high-capacity ship purpose-built for the Cebu-Palawan sea corridor. Offers multiple cabin classes, a sun deck, and onboard retail shops for a pleasant long-haul journey.', N'ship8.jpg', 8, N'9:00 AM');

SELECT Id, Name, Route, DepartureTime, Price FROM Ships ORDER BY RouteId, Route;
