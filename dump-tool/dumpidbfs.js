function dumpIdbfs(dbName, storeName, rootPath) {
	storeName = storeName || "FILE_DATA";
	let dbRequest = indexedDB.open(dbName || "/idbfs");

	dbRequest.onerror = (event) => {
		console.log("error opening idbfs db");
	};

	dbRequest.onsuccess = (event) => {
		let db = dbRequest.result;
		
		let transaction = db.transaction(storeName, "readonly");
		let objectStore = transaction.objectStore(storeName);

		let collection = {};
		let cursorRequest = objectStore.openCursor();
		cursorRequest.onerror = function(event) {
			console.err("error opening cursor for idbfs");
		};
		cursorRequest.onsuccess = function(event) {
			let cursor = event.target.result;
			if (cursor) {
				const key = cursor.primaryKey;
				const value = cursor.value;
				if(!rootPath || key.startsWith(rootPath)) {
					if(value.contents) {
						value["$contentsType"] = value.contents[Symbol.toStringTag];
					}
					collection[key] = value;
				}						
				cursor.continue();
			}
			else {
				cursorIterationDone();
			}
		};

		function cursorIterationDone() {
			let jsonData = JSON.stringify(collection);
			let blob = new Blob([jsonData], { type: "text/plain" });
			let a = document.createElement("a");
			a.download = "idbfs.json";
			a.href = window.URL.createObjectURL(blob);
			a.click();
		}
		
	};
}