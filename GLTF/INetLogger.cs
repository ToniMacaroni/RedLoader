using System.Globalization;
using UnityEngine;
using Object = System.Object;

namespace Barbershop.gltf;

public interface INetLogHandler
{
    void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args);

    void LogException(Exception exception, UnityEngine.Object context);
}


public interface INetLogger
{
    INetLogHandler logHandler { get; set; }

    bool logEnabled { get; set; }

    void Log(LogType logType, object message);

    void Log(LogType logType, object message, Object context);

    void LogError(string tag, object message);

    void LogFormat(LogType logType, string format, params object[] args);
}

public class NetLogger : INetLogger, INetLogHandler
{
	public INetLogHandler logHandler { get; set; }

	public bool logEnabled { get; set; }

	public LogType filterLogType { get; set; }

	public NetLogger(INetLogHandler logHandler)
	{
		this.logHandler = logHandler;
		logEnabled = true;
		filterLogType = LogType.Log;
	}

	public bool IsLogTypeAllowed(LogType logType)
	{
		if (logEnabled)
		{
			if (logType == LogType.Exception)
			{
				return true;
			}
			if (filterLogType != LogType.Exception)
			{
				return logType <= filterLogType;
			}
		}
		return false;
	}

	private static string GetString(object message)
	{
		if (message == null)
		{
			return "Null";
		}
		if (message is IFormattable formattable)
		{
			return formattable.ToString(null, CultureInfo.InvariantCulture);
		}
		return message.ToString();
	}

	public void Log(LogType logType, object message)
	{
		if (IsLogTypeAllowed(logType))
		{
			logHandler.LogFormat(logType, null, "{0}", GetString(message));
		}
	}

	public void Log(LogType logType, object message, object context)
	{
		if (IsLogTypeAllowed(logType))
		{
			logHandler.LogFormat(logType, context as UnityEngine.Object, "{0}", GetString(message));
		}
	}

	public void Log(LogType logType, object message, UnityEngine.Object context)
	{
		if (IsLogTypeAllowed(logType))
		{
			logHandler.LogFormat(logType, context, "{0}", GetString(message));
		}
	}

	public void Log(object message)
	{
		if (IsLogTypeAllowed(LogType.Log))
		{
			logHandler.LogFormat(LogType.Log, null, "{0}", GetString(message));
		}
	}

	public void LogError(string tag, object message)
	{
		if (IsLogTypeAllowed(LogType.Error))
		{
			logHandler.LogFormat(LogType.Error, null, "{0}: {1}", tag, GetString(message));
		}
	}

	public void LogException(Exception exception, UnityEngine.Object context)
	{
		if (logEnabled)
		{
			logHandler.LogException(exception, context);
		}
	}

	public void LogFormat(LogType logType, string format, params object[] args)
	{
		if (IsLogTypeAllowed(logType))
		{
			logHandler.LogFormat(logType, null, format, args);
		}
	}

	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		if (IsLogTypeAllowed(logType))
		{
			logHandler.LogFormat(logType, context, format, args);
		}
	}
}

public class RelayedLogger : INetLogger
{
	public static RelayedLogger Instance = new(UnityEngine.Debug.unityLogger);
	
	ILogger unityLogger;
	
	public RelayedLogger(ILogger logger)
	{
		unityLogger = logger;
	}

	public INetLogHandler logHandler { get; set; }
	public bool logEnabled { get; set; }
	public void Log(LogType logType, object message)
	{
		unityLogger.Log(logType, message.ToString());
	}

	public void Log(LogType logType, object message, object context)
	{
		unityLogger.Log(logType, message.ToString());
	}

	public void LogError(string tag, object message)
	{
		unityLogger.LogError(tag, message.ToString());
	}

	public void LogFormat(LogType logType, string format, params object[] args)
	{
		unityLogger.Log(logType, string.Format(format, args));
	}
}