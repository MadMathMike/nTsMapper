# nTsMapper
Simple tool to transpile DTO classes used in WebAPI action methods to TypeScript.

## Basic Information
This application will inspect the specified .NET dll for WebAPI controller methods to produce TypeScript code for all the command parameter and DTO classes used.

Command Line Parameters:
[-assemblypath] [-outputfilename] [-help] [-debug]

| Argument | Description |
| ------------ | ---------------- |
| help	| Displays this help message. |
| debug | Produces helpful debug information as comments in the generated TypeScript code. |
| assemblypath | Specifies the full path and filename to the .NET library to be inspected. Note: this parameter is REQUIRED and must be wrapped in quotation marks if the path contains a space. |
| outputfilename | Specifies the full path and file name of the output file where generated TypeScript code will be saved.  If this parameter is omitted, generated TypeScript codewill be sent to standard output.  Note: this parameter must be wrapped in quotation marks if the path contains a space. |

## Advanced Information

### Parameter Classes

**nTsMapper** is able to find all the classes used as parameters in query or command API methods through reflection.  For example:

```c#
public HttpResponseMessage DeleteCustomer(CustomerCommandParameter commandParameter)
```

In this example the ```CustomerCommandParameter``` parameter will be detected and a corresponding ```ICustomerCommandParameter``` interface and a ```CustomerCommandParameter``` class will be produced.

### DTO Classes

In order for **nTsMapper** to find classes used in API method responses, they must be included in a ```ReponseType``` attribute on the method where they are used.  For example:

```c#
[ResponseType(typeof(CustomerDeleteResult))]
public HttpResponseMessage DeleteCustomer(CustomerCommandParameter commandParameter)
{
	return Request.CreateResponse(HttpStatusCode.OK, new CustomerDeleteResult());
}
```

In this example, **nTsMapper** will find both the ```CustomerDeleteResult``` and the ```CustomerCommandParameter``` classes.

**Note**: the ```ResponseType``` attribute is declared in WebAPI 2.0 and higher in System.Web.Http.Description.

## TypeScript

Important things to point out about the generated TypeScript:
* An Interface and a Class are produced for each source .NET class
* Each TypeScript class has a fromJSON method
* Each interface/class combination is exported in a module name based on the class's original namespace.

### Interfaces and Classes

**nTsMapper** produces both an interface and a class for each source class to provide maximum flexibility client side.

The interface definition is provided for situations when you need to implement the interface in a separate class.

Class definitions are provided for use all over the client side including for example creating services that handle GET, SEND, or POST requests.  For example:

```TypeScript
getCustomer = (customerId: string): IPromise<WebApi.Models.Customers.ICustomerDashboardDTO> => {
	var deferred = this._q.defer();
	var apiUrl = this._baseApiUrl + 'getCustomer?customerId=' + customerId;

	this._http({ method: 'GET', url: apiUrl })
		.success((data: any) => {
			var response = WebApi.Models.Customers.fromJSON(data);
			deferred.resolve(response);
		})
		.error((data, status) => {
			var result = { status: status, errorMessage: "An error occurred. Please try again." };
			deferred.reject(result);
		});

	return deferred.promise;
};
```

### fromJSON Method

The fromJSON method allows the client side user to get the JSON data from a GET method, for example, and easily convert it into an instance of the corresponding TypeScript class with all the properties filled out.

```TypeScript
var response = WebApi.Models.Customers.fromJSON(data);
```

### Modules

**nTsMapper** places the generated TypeScript interface and class definition in their own module based on the namespace of the source class.  In many cases, this will result in multiple modules of the same name; however this allows **nTsMapper** to produce TypeScript code with class hierarchies that will be correctly interpreted by the JavaScript compiler.

## Sample Generated TypeScript

```TypeScript
module WebApi.Models.Customers {
	export interface ICustomerDeleteResult   {
	}
	export class CustomerDeleteResult  implements ICustomerDeleteResult {
		public static fromJSON(json: any) : CustomerDeleteResult {
			if (json === undefined)
				return undefined;
			if (json === null)
				return null;

			return {

			};
		}
	}
}

module WebApi.Models.Customers {
	export interface ICustomerCommandParameter   {
		id: string;
	}
	export class CustomerCommandParameter  implements ICustomerCommandParameter {
		id: string;
		public static fromJSON(json: any) : CustomerCommandParameter {
			if (json === undefined)
				return undefined;
			if (json === null)
				return null;

			return {
				id: json.id
			};
		}
	}
}

module WebApi.Models.Customers {
	export interface ICustomerDashboardDTO   {
		firstName: string;
		lastName: string;
		gender: string;
		id: string;
	}
	export class CustomerDashboardDTO  implements ICustomerDashboardDTO {
		firstName: string;
		lastName: string;
		gender: string;
		id: string;
		public static fromJSON(json: any) : CustomerDashboardDTO {
			if (json === undefined)
				return undefined;
			if (json === null)
				return null;

			return {
				firstName: json.firstName,
				lastName: json.lastName,
				gender: json.gender,
				id: json.id
			};
		}
	}
}
```


## License
[MIT](https://opensource.org/licenses/MIT)

## Dependencies
None