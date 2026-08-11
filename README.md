# Syncfusion React Pivot Table – SQLite Database Binding Sample

End-to-end sample that demonstrates how to bind a **SQLite** database to the Syncfusion React Pivot Table and perform **CRUD** (Create, Read, Update, Delete) operations through an ASP.NET Core Web API backend.

## Overview

This project shows how to:

- Connect a Syncfusion React PivotTable to a SQLite database.
- Expose data through an ASP.NET Core Web API that uses `Microsoft.Data.Sqlite`.
- Load, aggregate, and pivot sales data in the browser.
- Insert, update, and delete records via the API and reflect the changes immediately in the PivotTable.

## Prerequisites

- [Node.js](https://nodejs.org/) 18+ and npm
- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later
- A modern web browser (Edge, Chrome, Firefox, or Safari)

## Project Structure

```
syncfusion-react-pivot-table-sqlite-database-binding-sample/
├── README.md
├── PivotTable_SQLite.Server/      # ASP.NET Core Web API (SQLite + CRUD)
│   ├── Program.cs
│   ├── Controller/SalesController.cs
│   ├── appsettings.json
│   └── PivotTable_SQLite.Server.csproj
└── pivottable_sqlite.client/      # React + Vite + Syncfusion PivotTable
    ├── index.html
    ├── package.json
    ├── vite.config.ts
    └── src/
        ├── App.tsx
        ├── main.tsx
        └── App.css
```

The SQLite database file (`sales.db`) is created on first run inside the API project's output folder.

## Backend – ASP.NET Core Web API (SQLite)

The backend uses the official `Microsoft.Data.Sqlite` package to read and write the `Sales` table.

### Key Packages

- `Microsoft.Data.Sqlite`
- `Microsoft.AspNetCore.Cors`

### `SalesController` Endpoints

| Method | Route                    | Description                          |
| ------ | ------------------------ | ------------------------------------ |
| GET    | `/api/sales`             | Fetch all sales records              |
| POST   | `/api/sales`             | Insert a new sales record            |
| PUT    | `/api/sales/{id}`        | Update an existing sales record      |
| DELETE | `/api/sales/{id}`        | Delete a sales record                |

### Run the API

```bash
cd PivotTable_SQLite.Server
dotnet restore
dotnet run
```

By default the API listens on the URL configured in `Properties/launchSettings.json` (for example `https://localhost:7001` or `http://localhost:5000`).

## Frontend – React + Syncfusion PivotTable

The client is built with **Vite**, **React**, and **TypeScript** and uses the Syncfusion React PivotTable component.

### Key Packages

- `@syncfusion/ej2-react-pivotview`
- `axios`

### `App.tsx` Highlights

- Configures a remote data source pointing to the Web API endpoint (`/api/sales`).
- Defines rows, columns, values, and filters in the PivotView.
- Uses CRUD actions (`add`, `edit`, `delete`) to sync local changes back to SQLite through the API.

### Run the Client

```bash
cd pivottable_sqlite.client
npm install
npm run dev
```

Open the URL printed by Vite (typically `http://localhost:5173`) in your browser.

## Configuring the API URL in the Client

The Vite dev server proxies API requests to the ASP.NET Core backend. The proxy is configured in `pivottable_sqlite.client/vite.config.ts`:

```ts
server: {
  proxy: {
    '/api': {
      target: 'https://localhost:7001',
      changeOrigin: true,
      secure: false,
    },
  },
}
```

Update the `target` to match the URL where the API is running.

## How It Works

1. The API opens (or creates) `sales.db` and ensures the `Sales` table exists.
2. The React app fetches sales data from `/api/sales` and feeds it to the PivotTable.
3. When the user adds, edits, or deletes a cell/value, the client sends the corresponding HTTP request to the API.
4. The API executes the SQL command against SQLite and returns the updated dataset.
5. The PivotTable refreshes to reflect the latest data.

## CRUD with Syncfusion PivotTable

The PivotTable is configured to use the `editSettings` and CRUD action handlers that call the API:

```ts
editSettings: {
  allowAdding: true,
  allowEditing: true,
  allowDeleting: true,
  mode: 'Dialog',
},
actionBegin: (args) => {
  // Route add/edit/delete events to axios calls against /api/sales
}
```

## SQLite Schema

The `Sales` table is created automatically on first run with the following columns:

| Column      | Type    | Description                |
| ----------- | ------- | -------------------------- |
| Id          | INTEGER | Primary key (autoincrement)|
| Product     | TEXT    | Product name               |
| Category    | TEXT    | Product category           |
| Region      | TEXT    | Sales region               |
| Country     | TEXT    | Sales country              |
| Salesperson | TEXT    | Salesperson name           |
| Quantity    | INTEGER | Units sold                 |
| UnitPrice   | REAL    | Price per unit             |
| Revenue     | REAL    | Total revenue              |
| OrderDate   | TEXT    | Order date (ISO 8601)      |

## Troubleshooting

- **CORS errors:** Ensure the API has CORS enabled (see `Program.cs`) and the Vite proxy matches the API URL.
- **Empty PivotTable:** Confirm the API is running and accessible at the configured `target` URL.
- **SQLite file locked:** Stop the API before deleting `sales.db` to recreate the schema.

## See Also

- [Syncfusion React PivotTable Documentation](https://ej2.syncfusion.com/react/documentation/pivotview/getting-started)
- [Microsoft.Data.Sqlite Documentation](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
